module MyCheckers.WebApp

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket

open Domain
open Storage

module JsonSerializer =
  open System.Text
  open System.Text.Json
  open System.Text.Json.Serialization

  type private E = JsonUnionEncoding
  let encoding = E.ExternalTag ||| E.UnwrapFieldlessTags ||| E.UnwrapSingleFieldCases ||| E.UnwrapOption
  let private options = JsonSerializerOptions(WriteIndented = false, PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
  options.Converters.Add(JsonFSharpConverter(encoding, unionTagNamingPolicy = JsonNamingPolicy.CamelCase))

  let serialize obj = 
      JsonSerializer.Serialize(obj, options) 
      |> Encoding.UTF8.GetBytes
      |> ByteSegment

  let deserialize<'t> data =  
      JsonSerializer.Deserialize<'t>(UTF8.toString data, options) 

type WebSocket with
  member this.sendByChunks chunkSize message = socket {

    do! this.send Text ByteSegment.Empty false

    let segments = 
      message
      |> Seq.chunkBySize chunkSize 
      |> Seq.map ByteSegment

    for segment in segments do
      do! this.send Continuation segment false

    do! this.send Continuation ByteSegment.Empty true
  }

let games = GameStorage()
let sessions = SessionStorage()

let gameSession (gameId : string) (webSocket : WebSocket) (_ : HttpContext) =
  let heartbeat =
      async {
        while true do
          do! webSocket.send Ping ByteSegment.Empty true |> Async.Ignore
          do! Async.Sleep 30_000 // ms
      }
  let cts = new System.Threading.CancellationTokenSource()
  Async.Start(heartbeat, cts.Token)

  socket {
    sessions.Connect(gameId, webSocket) |> ignore

    let game = games.[gameId]

    do! webSocket.sendByChunks 16 (JsonSerializer.serialize game.State)

    let mutable loop = true
    while loop do
      let! msg = webSocket.read()

      // Opcode type:
      //   type Opcode = Continuation | Text | Binary | Reserved | Close | Ping | Pong
      match msg with
      | (Text, data, true) ->
        let act = JsonSerializer.deserialize<Act> data

        game.Update(act) |> ignore

        for socket in sessions.GetConnections(gameId) do
          try
            do! socket.sendByChunks 16 (JsonSerializer.serialize game.State)
          with
            | _ -> loop <- false
          
      | (Close, _, _) -> loop <- false

      | _ -> ()

    cts.Cancel()
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