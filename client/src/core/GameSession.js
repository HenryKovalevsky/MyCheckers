import Consts from "./Constants.js"

function getColumnsRows(player) {
  if (player == Consts.player.BLACK)
    return {
      columns: Consts.columns,
      rows: Consts.rows
    };
  else
    return {
      columns: Consts.columns.slice().reverse(),
      rows: Consts.rows.slice().reverse()
    }
}

function mapGameBoard(player, board) {
  const { columns, rows } = getColumnsRows(player);

  return board.map((item) => {
    let [[column, row], piece] = item;

    const x = columns.findIndex((i) => i == column);
    const y = rows.findIndex((i) => i == row);

    return {
      x: x,
      y: y,
      rank: piece.rank,
      player: piece.player,
      id: piece.id,
      isSelected: false
    };
  });
}

Array.prototype.except = function (other, comparer) {
  return this.filter(ai => !other.some(bi => comparer(ai, bi)));
}

function diffBoards(prev, updated) {
  let toAdd = updated.except(prev, ((a, b) => a.id == b.id));
  let toDelete = prev.except(updated, ((a, b) => a.id == b.id));
  let toUpdate = updated.except(prev, ((a, b) => a == b)).except(toAdd, ((a, b) => a == b))

  return {
    toAdd: toAdd,
    toUpdate: toUpdate,
    toDelete: toDelete,
  };
}

export function initGameSession(player, wsUrl, stateRef) {
  var websocket = new WebSocket(wsUrl);

  websocket.onopen = (event) => console.log(event);
  websocket.onclose = (event) => console.log(event);
  websocket.onerror = (event) => console.log(event);

  websocket.onmessage = (event) => {
    let state = JSON.parse(event.data);

    stateRef.currentPlayer = state.currentPlayer;
    stateRef.status = state.status;

    const board = mapGameBoard(player, state.board);
    const diff = diffBoards(stateRef.board, board);

    diff.toAdd.forEach((piece) => {
      stateRef.board.push(piece)
    });

    diff.toUpdate.forEach((piece) => {
      const index = stateRef.board.findIndex((p) => p.id == piece.id);
      stateRef.board.splice(index, 1, piece);
    });

    diff.toDelete.forEach((piece) => {
      const index = stateRef.board.findIndex((p) => p.id == piece.id);
      stateRef.board.splice(index, 1);
    });

    console.log(stateRef);
  };

  const move = (from, to) => {
    const message = JSON.stringify({ move: { from, to } });
    websocket.send(message);
  }

  const arrange = (to) => {
    const message = JSON.stringify({ arrange: to });
    websocket.send(message);
  }

  return {
    act(square) {
      if (player != stateRef.currentPlayer) return;

      const { columns, rows } = getColumnsRows(player);
      switch (stateRef.status) {
        case "draft":
          arrange([columns[square.x], rows[square.y]]);
          break;

        case "battle":
          let selected = stateRef.board.find((p) => p.isSelected);
          if (selected != null) {
            const from = [columns[selected.x], rows[selected.y]];
            const to = [columns[square.x], rows[square.y]];

            move(from, to);

            selected.isSelected = false;
          } else {
            let piece = stateRef.board.find(
              (p) => p.player == player // is player's turn
                && p.x == square.x // is piece on the square
                && p.y == square.y
            ) || {};

            piece.isSelected = true;
          }
          break;
      }
    }
  };
}
