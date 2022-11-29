# pip install pythonnet

dll_folder_path = r".\Api\bin\Debug\net6.0"

import sys
sys.path.append(dll_folder_path)

from pythonnet import load
load("coreclr")

import clr
clr.AddReference('MyCheckers.Domain')

from System import Tuple
from MyCheckers import Domain as D
from MyCheckers import Engine as E

class Game:
  game = E.Game()

  def arrange(self, column, row):
    square = Tuple.Create(column, row)
    return self.game.Update(D.Act.NewArrange(square))

  def move(self, column_from, row_from, column_to, row_to):
    from_square = Tuple.Create(column_from, row_from)
    to_square = Tuple.Create(column_to, row_to)
    move = D.ProposedMove(from_square, to_square)
    return self.game.Update(D.Act.NewMove(move))

  def getState(self):
    return self.game.State

  def getPossibleActs(self):
    return self.game.GetPossibleActs()

game = Game()

print(game.getState())

print(game.getPossibleActs())

# draft
game.arrange(D.Column.A, D.Row.One)
game.arrange(D.Column.A, D.Row.Eight)
game.arrange(D.Column.B, D.Row.One)
game.arrange(D.Column.B, D.Row.Eight)
game.arrange(D.Column.C, D.Row.One)
game.arrange(D.Column.C, D.Row.Eight)
game.arrange(D.Column.D, D.Row.One)
game.arrange(D.Column.D, D.Row.Eight)
game.arrange(D.Column.E, D.Row.One)
game.arrange(D.Column.E, D.Row.Eight)
game.arrange(D.Column.F, D.Row.One)
game.arrange(D.Column.F, D.Row.Eight)
game.arrange(D.Column.G, D.Row.One)
game.arrange(D.Column.G, D.Row.Eight)
game.arrange(D.Column.H, D.Row.One)
game.arrange(D.Column.H, D.Row.Eight)

game.arrange(D.Column.A, D.Row.Two)
game.arrange(D.Column.A, D.Row.Seven)
game.arrange(D.Column.H, D.Row.Two)
game.arrange(D.Column.H, D.Row.Seven)

print(game.getState())

# battle
game.move(D.Column.A, D.Row.Two, D.Column.A, D.Row.Three)
game.move(D.Column.A, D.Row.Seven, D.Column.A, D.Row.Six)
game.move(D.Column.A, D.Row.Three, D.Column.A, D.Row.Four)
game.move(D.Column.A, D.Row.Six, D.Column.A, D.Row.Five)

print(game.getState())

print(game.getPossibleActs())