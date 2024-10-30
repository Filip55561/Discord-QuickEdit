using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;

namespace QuickEdit.Commands;
internal sealed class InteractionServiceHandler(DiscordSocketClient client, InteractionService interactionService, InteractionServiceConfig interactionServiceConfig, IHostApplicationLifetime appLifetime) : IHostedService
{
    private readonly DiscordSocketClient _client = client;
    private readonly InteractionService _interactionService = interactionService;
    private readonly InteractionServiceConfig _interactionServiceConfig = interactionServiceConfig;
    private readonly IHostApplicationLifetime _appLifetime = appLifetime;
    private static readonly SemaphoreSlim _initSemaphore = new(1);
    private static bool isReady;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Ready += (() =>
        {
            for (int i = 0; i < 10; i++)
            {
                Task t = new(async () => await InitAsync());
                t.Start();
            }
            return Task.CompletedTask;
        });
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Detach event handlers
        _client.Ready -= (() =>
        {
            for (int i = 0; i < 10; i++)
            {
                Task t = new(async () => await InitAsync());
                t.Start();
            }
            return Task.CompletedTask;
        });
        
        _interactionService.SlashCommandExecuted -= OnSlashCommandExecutedAsync;
        _client.InteractionCreated -= OnInteractionCreatedAsync;
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Initialize the InteractionService
    /// </summary>
    private async Task InitAsync()
{
    Log.Information("InitAsync attempt started.");  // Log each call to InitAsync

    if (isReady) return;

    await _initSemaphore.WaitAsync();
    try
    {
        if (isReady) return;

        await RegisterModulesAsync();
        _interactionService.SlashCommandExecuted += OnSlashCommandExecutedAsync;
        _client.InteractionCreated += OnInteractionCreatedAsync;
        isReady = true;
        Log.Information("InteractionService initialized successfully.");
    }
    catch (Exception e)
    {
        Log.Fatal("Error initializing InteractionService: {Error}", e);
        _appLifetime.StopApplication();
    }
    finally
    {
        _initSemaphore.Release();
    }
}

    /// <summary>
    /// Register modules / commands
    /// </summary>
    private async Task RegisterModulesAsync()
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                await _interactionService.AddModulesAsync(assembly, null);
            }
            await _interactionService.RegisterCommandsGloballyAsync();
            Log.Information("Modules registered successfully.");
        }
        catch (Exception e)
        {
            Log.Fatal("Error registering modules: {Error}", e);
            throw;
        }
    }

    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(context, null);
        }
        catch (Exception e)
        {
            Log.Error("Error handling interaction: {Error}", e);

            if (interaction.Type is InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
            }
        }
    }

    private static async Task OnSlashCommandExecutedAsync(SlashCommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess) return;

        try
        {
            Log.Error("Error executing command '{Command}': {ErrorReason}", commandInfo?.Name, result.ErrorReason);
            await context.Interaction.FollowupAsync("An error occurred while executing the command.", ephemeral: true);
        }
        catch (Exception e)
        {
            Log.Error("Error handling interaction exception: {Error}", e);
            throw;
        }
    }
}
