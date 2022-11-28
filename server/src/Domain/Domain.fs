module Domain

type Column = | A | B | C | D | E | F | G | H
type Row = | One | Two | Three | Four | Five | Six | Seven | Eight 

module Column =
  let list = [ A; B; C; D; E; F; G; H ]
module Row = 
  let list = [ One; Two; Three; Four; Five; Six; Seven; Eight ]

type Square = Column * Row

type Color = | White | Black
type Rank = | Man | King
type GameStatus = | Draft | Battle | Draw | Victory of Color

module Color =
  let rev = function
    | White -> Black
    | Black -> White

type Piece = 
  { Player: Color
    Rank: Rank
    Id: int }

type Board = Map<Square, Piece>

module Board =
  let getValue (board : Board) (square : Square) = 
    match square, board.TryGetValue square with
    | square, (true, piece)-> square, Some piece 
    | square, (false, _) -> square, None

type ProposedMove =
  { From: Square
    To: Square }

type Act =
  | Arrange of Square
  | Move of ProposedMove

type Move =
  { Piece: Piece
    From: Square
    To: Square
    CapturedPiece: Square option }

type GameState =
  { Board: Board
    CurrentPlayer: Color
    Status: GameStatus
    MoveHistory: Move list } 

let [<Literal>] PiecesCount = 20
let [<Literal>] ArrangeRows = 3