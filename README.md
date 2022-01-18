# Splinterlands RO Bot
![banner](https://d36mxiodymuqjm.cloudfront.net/website/home/bg_home_hero_chaos.jpg)

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
* Pro features:
	- Bot can collect Hive SPS Airdrops
	- Transfer Bot for transfering assets to one main account (Cards, Dec, SPS)
	- Bot will try to predict the enemy team to increase the chances of winning !experimental feature
	- Quest rewards export to Excel for the past 30 days

## Console Commands
* START-TRANSFER-BOT (Starts the transfer process from all the accounts to the main accout)
* START-QUEST-REWARDS-EXPORT (Start the export to Excel process for all the accounts)
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
* USE_ENEMY_PREDICTION=true/false
* TRANSFER_BOT_MAIN_ACCOUNT=TEXT(USERNAME)
* TRANSFER_BOT_SEND_CARDS=true/false
* TRANSFER_BOT_SEND_DEC=true/false
* TRANSFER_BOT_KEEP_DEC_AMOUNT=NUMBER
* TRANSFER_BOT_MINIMUM_DEC_TO_TRANSFER=NUMBER
* TRANSFER_BOT_SEND_SPS=true/false

## Setup guide
- Download the lates version from Release page
- Unzip the downloaded file
- Open users.xml with your favorite text editor and fill the required user data
- Open config.xml with your favorite text editor and adjust the settings to your needs
- Linux Only! Run ****sudo chmod +x SplinterlandsRObot**** before starting the bot
- Run the SplinterlandsRObot file

## Support & Community
[Discord](https://discord.gg/PrqxhN6d9j)
