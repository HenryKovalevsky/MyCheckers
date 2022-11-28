module Storage

open System.Collections.Concurrent

open Suave.WebSocket

open Engine

type GameStorage() = 
  let games = new ConcurrentDictionary<string, Game>()

  member _.CreateGame() : string =
    let game = Game()
    let gameId = $"{game.Uid:N}"
    games.[gameId] <- game
    gameId

  member _.Item with get(gameId : string): Game = games.[gameId]

type SessionStorage() =
  let connections = new ConcurrentDictionary<string, WebSocket list>()

  member _.Connect(sessionId : string, webSocket : WebSocket) : unit =
    connections.AddOrUpdate(sessionId, [webSocket], fun _ sockets -> webSocket::sockets)
    |> ignore

  member _.GetConnections(sessionId : string) : WebSocket list =
    connections.[sessionId]

  member _.Disconnect(sessionId : string, webSocket : WebSocket) : unit =
    let sockets = connections.AddOrUpdate(sessionId, [], fun _ sockets -> List.filter ((<>)webSocket) sockets)
    if Seq.isEmpty sockets then connections.TryRemove(sessionId, ref sockets) |> ignore