# Reddit Responder Bot (TedCruzResponderBot)

This is a small proof of concept inspired by [this comment](https://old.reddit.com/r/MurderedByAOC/comments/mi1yar/this_is_pretty_rich/gt28m7p/) 
on Reddit. This application is capable of monitoring a list of subreddits for comments containing any number of trigger 
strings. When a comment with a matching trigger is detected, the bot with find an applicable template and respond with 
the template's configured response.

# Getting Started

## Prerequisites

- You will need a [Reddit Account](https://www.reddit.com) to run this application with.
- [.NET 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)
- *OPTIONAL* -  [Docker Compose](https://docs.docker.com/compose/install/)

## Getting the project

To get started, first clone the project with:

```bash
https://github.com/JerrettDavis/RedditTedCruzResponderBot.git
```

Next, jump into the newly created directory:

```bash
cd TedCruzResponderBot
```

## Configuring the project

Once you're in the solution directory, you'll need to configure the application to use your Reddit account. To do this,
you'll need an `AppId` and a `RefreshToken`. If you've never setup an app on Reddit before, check out [this guide](https://www.youtube.com/watch?v=xlWhLyVgN2s)
from the [Reddit.NET](https://github.com/sirkris/Reddit.NET) library creators. It utilizes the `AuthTokenRetriever` 
utility within the Reddit.NET project.

Once you have your credentials, you will need to place them one of two places, depending on how you wish to run the 
application.

### Docker

To run the application under docker, create a new filed called `.env`. The contents of the files will 
include the following: 

```dotenv
AppId=YOUR_ID_HERE
RefreshToken=YOUR_REFRESH_TOKEN_HERE
```

### Dotnet 

To run the application from the commandline (or your IDE) using `dotnet`, update the `AppId` and `RefreshToken` 
properties in `appsettings.json` like so:

```json
{
  /// ...
  
  "AppId": "YOUR_ID_HERE",
  "RefreshToken": "YOUR_REFRESH_TOKEN_HERE",
  
  /// ...
}
```

### Configuring Monitoring

The default behavior for the application is to store its monitoring configuration in-memory. To change this configuration,
you must adjust the appropriate settings in `appsettings.json`.

#### Setting Monitored Subreddits

To adjust the list of subreddits the bot should monitor, adjust the `Subreddits` property in `appsettings.json` like so:

```json
{
  // ...

  "Subreddits": [
    "askreddit",
    "politics",
    "HydroHomies"
  ]

  // ...
}
```

#### Setting Trigger Templates

Each trigger template contains 3 standard properties: 

- `TemplateName` - `[string]` - Currently unused by the application. It makes tracking templates easier.
- `Triggers` - `[IEnumerable<string>]` - A collection of strings you wish to search for.
- `Response` - `[string]` - The complete comment you want the bot to respond to triggered comments with.

To adjust the list of templates the bot uses, adjust the `Templates` property in `appsettings.json` like so:

```json
{
  // ...
  
  "Templates": [
    {
      "TemplateName": "Agree with me",
      "Triggers": ["Isn't that right, bot?"],
      "Response": "Yup. You're 100% correct. \n\n---\n^I'm ^a ^bot, ^and ^this ^was ^an ^automated ^action."
    }
  ]
  
  // ...
}
```

## Running the Bot

There are 2 primary ways to run the bot: Docker and dotnet-cli. 

### Docker

To run the bot under docker, run the following commands from the root solution directory:

```bash
docker-compose build
docker-compose up
```

The application should start up and start spitting out logs. To shut the application back down, close out of the session and then run:

```bash
docker-compose down
```

### Dotnet Cli

To run the bot with the dotnet cli, run the following commands from the root solution directory:

```bash
dotnet restore
dotnet build
dotnet run --project ./src/Bot.Service
```

# Warnings and Limitations

This application relies on the [Reddit API](https://www.reddit.com/dev/api), and as such, you're bound by Reddit's terms
of service and their [API access rules](https://github.com/reddit-archive/reddit/wiki/API). 

One core limitation of the Reddit API is that it only allows up to 60 requests a minute. If you are monitoring a large
number of subs, 

