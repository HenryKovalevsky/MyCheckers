# Henry's checkers

There is my variation of checkers that I invented long time ago.

I just remember that as a kid, I used to play chess and draughts with a friend of mine sometimes.

And one day I decided to change the rules of draughts to make it more fun.

I was reminded of those days and I wanted to implement the game since I can remember.

## Game

The exact rules and demo can be found [here](https://checkers.henrykovalevsky.space).

## Prerequisites

Written in [F#](https://fsharp.org) and [Vue](https://vuejs.org).

- [.NET SDK](https://dotnet.microsoft.com/) to work with F# files and dependencies;
- [Node.js](https://nodejs.org/) to execute JS code.

## How to use

- `build.bat` — build for production.

### Server

- `cd server`
- `dotnet build` — install dependencies and build;
- `dotnet run --project src\Api\MyCheckers.Api.fsproj` — starts dev server.

### Client

- `cd client`
- `npm ci` — install dependencies;
- `npm run dev` — start dev server.