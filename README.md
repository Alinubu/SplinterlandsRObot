# Splinterlands RO Bot - [Discord](https://discord.gg/PrqxhN6d9j)
![banner](https://d36mxiodymuqjm.cloudfront.net/website/home/bg_home_hero_chaos.jpg)

Splinterlands RO Bot is a fast Battle-API interaction based bot.

## Bot features
* Quests: 
	- Bot will prioritize teams based on active quest.
	- You can set a list of quests that you want the bot to avoid and it will request a new quest when possible
	- Bot can claim quest rewards
* Season:
	- Auto claim season rewards
* Battle:
	- Fast battles no webdriver needed
	- Multiple accounts can play in parallel
	- Energy capture rate limit
	- Bot can wait for the ECR to recharge to a specific value befor starting to play again
	- Option to set a list of preferred summoners that the bot will try to use when chosing teams
	- Replacement of starter cards with similar higher level cards from the user collection if rulesets allow it
* Pro features:
	- Bot can collect Hive SPS Airdrops
	- Claim SPS rewards
	- Unstake SPS
	- Transfer Bot for transfering assets to one main account (Cards, Dec, SPS, Packs)
	- Quest rewards export to Excel for the past 30 days
	- DEC rewards export to Excel
	- Card renting
	- Online stats dashboard

## Console Commands
* START-TRANSFER-BOT (Starts the transfer process from all the accounts to the main account)
* START-QUEST-REWARDS-EXPORT (Start the export to Excel process for all the accounts)
* START-DEC-REWARDS-EXPORT (Start the export to Excel for all the accounts for the last 14 days)
* START-CLAIM-SEASON-REWARDS (Starts the process for claiming Season Rewards)

Settings that can be changed withoud restarting the application (**Changes will not be saved to config file!**)
* HOLD_CACHE_FOR=NUMBER
* DEBUG_MODE=true/false

## Setup guide
- Download the lates version from [Release page](https://github.com/Alinubu/SplinterlandsRObot/releases)
- Unzip the downloaded file
- Open users_example.xml with your favorite text editor and fill the required user data and save it as users.xml
- Open config_example.xml with your favorite text editor and adjust the settings to your needs and save it as config.xml
- Open settings_example.xml with your favorite text editor and adjust the settings to your needs and save it as settings.xml
- Linux/Mac Only! Run ****sudo chmod +x SplinterlandsRObot**** before starting the bot
- Run the SplinterlandsRObot file

## Support & Community
[Discord](https://discord.gg/PrqxhN6d9j)
