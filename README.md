﻿# MatsueNet C# Discord Bot
## Why I created Matsue
Originally, I was learning nodejs and made a discord bot in js. However, I left that project unfinished for a long time and recently I wanted to learn C# because its the language I will be writing my final year project in. So by writing a bot in C# it allowed me to continue the development of my dormant bot and, learn a new language.

## Database structure
The database is based on MongoDB. There are currently two documents (tables) one is to hold the data for each user and another is to hold the server data.

#### Users document 
```json
{
    "_id": {
        "$oid": "Unique entry id"
    },
    "user_id": {
        "$numberLong": "01010101010101010"
    },
    "married": false,
    "married_to": {
        "$numberLong": "01010101010101010"
    },
    "balance": {
        "$numberLong": "0"
    }
}
```
#### Guilds document
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
    "bot_channel": null,
    "admin_channel": null,
    "prefix": "!"
```

## Todo List
- [ ] Implement checks to make sure commands are only executed in specefic channels if the bot it setup that way
- [ ] Implement marriage system
  - [ ] Allow people to see who someone is married
  - [ ] Only allow 1 across all of discord (global marraige)
- [ ] Implement balances
  - [ ] View balance
  - [ ] Get money (dailys or something like that)
  - [ ] Send money
  - [ ] Recieve money
- [ ] More features to think of... 




