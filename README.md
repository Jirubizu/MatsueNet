# MatsueNet C# Discord Bots

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/1b7cd4ea417c4db2ba8106775845171c)](https://app.codacy.com/manual/Jirubizu/MatsueNet?utm_source=github.com&utm_medium=referral&utm_content=Jirubizu/MatsueNet&utm_campaign=Badge_Grade_Dashboard)

## Why I created Matsue
Originally, I was learning nodejs and made a discord bot in js. However, I left that project unfinished for a long time and recently I wanted to learn C# because its the language I will be writing my final year project in. So by writing a bot in C# it allowed me to continue the development of my dormant bot and, learn a new language.

Test

## Database structure
The database is based on MongoDB. There are currently two documents (tables) one is to hold the data for each user and another is to hold the server data.

### Users document 
```json
{
    "_id": {
        "$oid": "Unique entry id"
    },
    "user_id": {
        "$numberLong": "01010101010101010"
    },
    "balance": 1.04
}
```
### Guilds document
```json
    "_id": {
        "$oid": "Unique entry id"
    },
    "guild_id": {
        "$numberLong": "01010101010101010"
    },
    "music_channel": {
        "$numberLong": "01010101010101010"
    },
    "bot_channel": {
        "$numberLong": "01010101010101010"
    },
    "admin_channel": {
        "$numberLong": "01010101010101010"
    },
    "prefix": "!"
```

## Todo List
- [x] Implement checks to make sure commands are only executed in specific channels if the bot it setup that way
- [x] Implement balances
  - [x] View balance
  - [x] Obtain money daily through messaging
  - [x] Send money
  - [x] Receive money
- [ ] More features to think of... 

## Prerequisites
 - Discord.Net
 - [Lavalink](https://github.com/Frederikam/Lavalink)
 - Victoria (Provided as its slightly customised)


