module MyCheckers.Engine

open System

open Domain

[<AutoOpen>]
module private Internal =

  let (|IsSelected|_|) (state : GameState, prop : ProposedMove) =
    Board.getValue state.Board prop.From 
    |> snd
    |> Option.map (fun piece -> piece.Player = state.CurrentPlayer)

  let (|IsValidMove|_|) (state : GameState, prop : ProposedMove) =
    let compare (prop : ProposedMove) (move : Move) = 
      prop.From = move.From && prop.To = move.To

    let captures = Moves.getPossibleCaptures state

    if not (Seq.isEmpty captures)
    then 
      Seq.tryFind (compare prop) captures
    else
      Moves.getPossibleMoves state
      |> Seq.tryFind (compare prop)

  let (|IsValidArrange|_|) (state : GameState, square : Square) =
    let (column, row) = square
    if state.Board.ContainsKey square
    then None
    else 
      Row.list 
      |> if state.CurrentPlayer = White then id else List.rev
      |> Seq.take ArrangeRows 
      |> Seq.tryFind ((=)row)
      |> Option.map (fun row -> column, row)

  let getPossibleActs (state : GameState) =
    match state.Status with
    | Draft -> 
        Row.list 
        |> Seq.ofList
        |> if state.CurrentPlayer = White then id else Seq.rev
        |> Seq.take ArrangeRows
        |> Seq.allPairs Column.list
        |> Seq.filter (not << state.Board.ContainsKey)
        |> Seq.map Arrange
        |> Seq.toList
    | Battle ->
        let captures = Moves.getPossibleCaptures state

        if not (Seq.isEmpty captures)
        then captures
        else Moves.getPossibleMoves state
        |> Seq.map (fun move -> Move { From = move.From; To = move.To })
        |> Seq.toList
    | _ -> []

  let isPromotionSquare (player : Color) ((_, row) : Square)=
    if player = White
    then Seq.last Row.list = row
    else Seq.head Row.list = row

  let handleArrange (state : GameState) (square : Square) =
    match state, square with
    | IsValidArrange square ->
      let index = Seq.length state.Board.Values + 1

      let piece =
        { Player = state.CurrentPlayer
          Rank = Man
          Id = index }

      let board =
        state.Board
        |> Map.add square piece
    
      let status = 
        if index >= PiecesCount
        then Battle
        else Draft

      let move = 
        { From = square
          To = square
          Piece = piece
          CapturedPiece = None }

      { Board = board
        CurrentPlayer = Color.rev state.CurrentPlayer
        Status = status
        MoveHistory = move::state.MoveHistory }
    | _ -> state
  
  let handleMove (state : GameState) (prop : ProposedMove) =
    match state, prop with
    | IsSelected true & IsValidMove move -> 
        let board =
          state.Board
          |> Map.remove move.From
          |> Map.add move.To move.Piece
          |> if move.CapturedPiece.IsSome 
              then Map.remove move.CapturedPiece.Value 
              else id

        let state = { state with Board = board }

        let continue' = 
          move.CapturedPiece.IsSome && Moves.getPossibleCaptures state |> Seq.exists (fun capture -> capture.From = move.To)

        let nextPlayer = 
          if continue'
          then state.CurrentPlayer
          else Color.rev state.CurrentPlayer

        let board =
          if not continue' && isPromotionSquare state.CurrentPlayer move.To
          then board |> Map.change move.To (fun _ -> Some { move.Piece with Rank = King })
          else board

        let status = 
          let (white, black) =
              state.Board.Values
                |> List.ofSeq
                |> List.partition (fun piece -> piece.Player = White)
          if Seq.isEmpty white
          then Victory Black
          elif Seq.isEmpty black
          then Victory White
          else Battle

        { Board = board
          CurrentPlayer = nextPlayer
          Status = status
          MoveHistory = move::state.MoveHistory }
    | _ -> state

  let updateGameState (state : GameState) (act : Act) = 
    match act with
    | Arrange square when state.Status = Draft -> handleArrange state square
    | Move move when state.Status = Battle -> handleMove state move
    | _ -> failwithf "Invalid act. State: %A" state

type Game() =
  let mutable state =
    { Board = Map.empty
      CurrentPlayer = White
      Status = Draft
      MoveHistory = [] }

  member val Uid = Guid.NewGuid()
  member _.State = state

  member _.Update(act : Act) =
    let prev = state
    state <- updateGameState state act
    prev <> state

  member _.GetPossibleActs() =
    getPossibleActs state