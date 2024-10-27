# CONTRIBUTION GUIDE

Contributions are welcome!\
If you've never contributed to a GitHub repo before, you can check out [this guide](https://docs.github.com/en/get-started/exploring-projects-on-github/contributing-to-a-project), I guess.

I'm not specifying any guidelines, because I have no idea how to write or create good guidelines.

If you have any questions, create an issue or something, and I'll try to respond.

## How to Set Up the Bot

1. **Go to the [Discord Developer Portal](https://discord.com/developers/applications)**.
2. **Create a New Application** and name it whatever you want.
3. **Navigate to the Bot Tab** on the left panel.
4. **Find TOKEN** and click on **Reset TOKEN**. (Make sure to store it somewhere safe!)
5. **Create a new file** called `config.json` in the same directory as the .exe file.
6. **Copy this into the `config.json` file**:

   ```json
   {
       "DiscordConfig": {
           "token": "",
           "debug": true
       }
   }
   ```

7. **Insert your Token** inside the quotes: `""`.
8. **Rebuild the project** If u now rebuild the project, it should work without **crashing**!

## Installing the Bot on Discord

1. **Return to the [Discord Developer Portal](https://discord.com/developers/applications)**.

2. **Locate Your Application**: Find and select the application you just created.
3. **Go to the Installation Tab**: Click on the "Installation" option in the left panel.
4. **Enable User Install**: Ensure that the **USER INSTALL** option is checked.
5. **Copy the Installation Link**: Find the installation link provided and paste it into your web browser.
6. **Authorize the Bot**: Click on **Authorize** to authorize the bot to your account.
