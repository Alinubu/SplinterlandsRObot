# Splinterlands RO Bot - [Discord](https://discord.gg/PrqxhN6d9j)
![banner](https://d36mxiodymuqjm.cloudfront.net/website/home/bg_home_hero_chaos.jpg)

Splinterlands RO Bot is a fast Battle-API interaction based bot, rebuilt from [Ultimate-Splinterlands-Bot -2](https://github.com/PCJones/Ultimate-Splinterlands-Bot-V2).

## Bot features
* Quests: 
	- Bot will prioritize teams based on active quest.
	- You can set a list of quests that you want the bot to avoid and it will request a new quest when possible
	- Bot can claim quest rewards
* Battle:
	- Fast battles due to blockchain interaction
	- Multiple accounts can play in parallel
	- Energy capture rate limit
	- Bot can wait for the ECR to recharge to a specific value befor starting to play again
	- Option to set a list of preferred summoners that the bot will try to use when chosing teams
	- Replacement of starter cards with similar higher level cards from the user collection if rulesets allow it
* Pro features:
	- Bot can collect Hive SPS Airdrops
	- Transfer Bot for transfering assets to one main account (Cards, Dec, SPS, Packs)
	- Quest rewards export to Excel for the past 30 days
	- DEC rewards export to Excel
	- Card renting
	- Online stats dashboard

## Console Commands
* START-TRANSFER-BOT (Starts the transfer process from all the accounts to the main accout)
* START-QUEST-REWARDS-EXPORT (Start the export to Excel process for all the accounts)
* START-DEC-REWARDS-EXPORT (Start the export to Excel for all the accounts for the last 14 days)
* START-CLAIM-SEASON-REWARDS (Starts the process for claiming Season Rewards)

Settings that can be changed withoud restarting the application (**Changes will not be saved to config file!**)
* HOLD_CACHE_FOR=NUMBER
* DEBUG_MODE=true/false
* SLEEP_BETWEEN_BATTLES=NUMBER
* SHOW_BATTLE_RESULTS=true/false
* ECR_LIMIT=NUMBER
* ECR_WAIT_TO_RECHARGE=true/false
* ECR_RECHARGE_LIMIT=NUMBER
* LEAGUE_ADVANCE_TO_NEXT=true/false
* DO_QUESTS=true/false
* COLLECT_SPS=true/false
* USE_PRIVATE_API=true/false
* TRANSFER_BOT_MAIN_ACCOUNT=TEXT(USERNAME)
* TRANSFER_BOT_SEND_CARDS=true/false
* TRANSFER_BOT_SEND_DEC=true/false
* TRANSFER_BOT_KEEP_DEC_AMOUNT=NUMBER
* TRANSFER_BOT_MINIMUM_DEC_TO_TRANSFER=NUMBER
* TRANSFER_BOT_SEND_SPS=true/false

## Setup guide
- Download the lates version from [Release page](https://github.com/Alinubu/SplinterlandsRObot/releases)
- Unzip the downloaded file
- Open users_example.xml with your favorite text editor and fill the required user data and save it as users.xml
- Open config_example.xml with your favorite text editor and adjust the settings to your needs and save it as config.xml
- Linux/Mac Only! Run ****sudo chmod +x SplinterlandsRObot**** before starting the bot
- Run the SplinterlandsRObot file

## Config settings
View the detailed configuration list [here](http://splinterlandsrobot.com/#!/BotConfigs)

## Support & Community
[Discord](https://discord.gg/PrqxhN6d9j)
