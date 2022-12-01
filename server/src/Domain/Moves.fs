module internal MyCheckers.Moves

open Domain

[<AutoOpen>]
module private Internal =
  type MoveDirection =
    | North    
    | NorthEast
    | East     
    | SouthEast
    | South    
    | SouthWest
    | West     
    | NorthWest

  module MoveDirection =
    let list = [ North; NorthEast; East; SouthEast; South; SouthWest; West; NorthWest ]
    let blackManList = [ West; SouthWest; South; SouthEast; East ]
    let whiteManList = [ West; NorthWest; North; NorthEast; East ]


  let manCapturePattern enemy = [ Some enemy; None ]
  let kingCapturePattern enemy = [ None; Some enemy; None ]

  let manMovePattern = [ None ]
  let kingMovePattern = [ None; None ]

  let next list current =
    list
    |> Seq.pairwise
    |> Seq.tryFind ((=)current << fst)
    |> Option.map snd

  let thisColumn = Some
  let nextColumn = next Column.list
  let prevColumn = next (List.rev Column.list)
  let thisRow = Some
  let nextRow = next Row.list
  let prevRow = next (List.rev Row.list)

  let nextSquare direction (column, row) : Square option =
    let nextColumn, nextRow = 
      match direction with
      | North     -> (thisColumn column, nextRow row) // ↑
      | NorthEast -> (nextColumn column, nextRow row) // ↗
      | East      -> (nextColumn column, thisRow row) // →
      | SouthEast -> (nextColumn column, prevRow row) // ↘
      | South     -> (thisColumn column, prevRow row) // ↓
      | SouthWest -> (prevColumn column, prevRow row) // ↙
      | West      -> (prevColumn column, thisRow row) // ←
      | NorthWest -> (prevColumn column, nextRow row) // ↖

    Option.map2 (fun a b -> a, b) nextColumn nextRow

  let getColor = function
    | Some piece -> Some piece.Player
    | None -> None

  let getMove (line : seq<Square * Piece option>) (pattern : #seq<Color option>) = 
    if Seq.forall2 (fun expected (_, piece) -> getColor piece = expected) pattern line
    then
      let target =
        line
        |> Seq.map2 (fun _ (square, _) -> square) pattern
        |> Seq.tryLast

      target
    else None

  let getCapture (line : seq<Square * Piece option>) (pattern : #seq<Color option>) = 
    if Seq.forall2 (fun expected (_, piece) -> getColor piece = expected) pattern line && Seq.length pattern <= Seq.length line
    then
      let target =
        line
        |> Seq.map2 (fun _ (square, _) -> square) pattern
        |> Seq.tryLast

      let captured = 
        line
        |> Seq.map2 (fun (captured: Color option) (square, _) -> square, captured) pattern
        |> Seq.tryFind (fun (_, captured) -> captured.IsSome)
        |> Option.map fst

      Option.map2 (fun a b -> a, b) target captured
    else None

  let getLines directions (board : Board) (square : Square) : seq<Square * Piece option> list = [
    let generator direction = Option.map (fun square -> square, square) << nextSquare direction

    for direction in directions do
      yield
        square
        |> Seq.unfold (generator direction)
        |> Seq.map (Board.getValue board)
  ]

let getPossibleCaptures (state : GameState) = seq {
  let enemy = Color.rev state.CurrentPlayer

  let pieces = 
    Map.filter (fun _ piece -> piece.Player = state.CurrentPlayer) state.Board

  for kvp in pieces do
    let square = kvp.Key
    let piece = kvp.Value

    for line in getLines MoveDirection.list state.Board square do
      let captures = Seq.choose id [
        yield getCapture line (manCapturePattern enemy)
        if piece.Rank = King then yield getCapture line (kingCapturePattern enemy)
      ] 

      for target, captured in captures do 
        yield { Piece = piece
                From = square 
                To = target
                CapturedPiece = Some captured }
}

let getPossibleMoves (state : GameState) = seq {
  let pieces = 
    Map.filter (fun _ piece -> piece.Player = state.CurrentPlayer) state.Board

  for kvp in pieces do
    let square = kvp.Key
    let piece = kvp.Value

    let directions = 
      if piece.Rank = Man && state.CurrentPlayer = White
      then MoveDirection.whiteManList
      elif piece.Rank = Man && state.CurrentPlayer = Black
      then MoveDirection.blackManList
      else MoveDirection.list

    for line in getLines directions state.Board square do
      let moves = Seq.choose id [
        yield getMove line manMovePattern
        if piece.Rank = King then yield getMove line kingMovePattern
      ] 

      for target in moves do 
      yield { Piece = piece
              From = square 
              To = target
              CapturedPiece = None }
}
  