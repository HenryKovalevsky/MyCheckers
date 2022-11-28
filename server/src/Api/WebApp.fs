module WebApp

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket

open Domain
open Storage

module Bytes =
  open System.Text
  open System.Text.Json
  open System.Text.Json.Serialization

  let private options = JsonSerializerOptions(WriteIndented = false, PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
  options.Converters.Add(JsonFSharpConverter(JsonUnionEncoding.InternalTag ||| JsonUnionEncoding.UnwrapFieldlessTags))

  let ser obj = JsonSerializer.Serialize(obj, options) 

  let serialize obj = 
      JsonSerializer.Serialize(obj, options) 
      |> Encoding.UTF8.GetBytes 

  let deserialize<'t> data =  
      JsonSerializer.Deserialize<'t>(UTF8.toString data, options) 

let games = GameStorage()
let sessions = SessionStorage()

let gameSession (gameId : string) (webSocket : WebSocket) (_ : HttpContext) =
  let emptyResponse = [||] |> ByteSegment

  let sendGameState game (webSocket : WebSocket) = socket {
    do! webSocket.send Text emptyResponse false

    let segments = 
      game
      |> Bytes.serialize 
      |> Seq.chunkBySize 16 
      |> Seq.map ByteSegment

    for segment in segments do
      do! webSocket.send Continuation segment false

    do! webSocket.send Continuation emptyResponse true
  }

  socket {
    sessions.Connect(gameId, webSocket) |> ignore
    
    let game = games.[gameId]

    do! sendGameState game.State webSocket

    let mutable loop = true
    while loop do
      let! msg = webSocket.read()

      // Opcode type:
      //   type Opcode = Continuation | Text | Binary | Reserved | Close | Ping | Pong
      match msg with
      | (Text, data, true) ->
        let act = Bytes.deserialize<Act> data

        let game = games.[gameId].Update(act)

        for socket in sessions.GetConnections(gameId) do
          try
            do! sendGameState game socket
          with
              | _ -> loop <- false

      | (Close, _, _) ->
        do! webSocket.send Close emptyResponse true
        loop <- false

      | _ -> ()

    sessions.Disconnect(gameId, webSocket)
  }

let createGame (ctx : HttpContext) =
  let gameId = games.CreateGame()
  OK gameId ctx

let app : WebPart = 
  choose [
    GET >=> path "/" >=> Files.browseFileHome "index.html"

    pathScan "/ws/%s" (handShake << gameSession)

    POST >=> path "/api/games" >=> createGame

    Files.browseHome
  ]