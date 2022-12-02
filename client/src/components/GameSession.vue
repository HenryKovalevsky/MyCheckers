<template>
  <GameBoard :player="player" :gameState="gameState" @move="move" @arrange="arrange" />

  <code style="display: flex; flex-direction: column; align-items: center">
    <pre v-if="gameState.status == 'Draft'">
      {{ gameState.status }} stage. {{ gameState.currentPlayer }}'s turn to set piece.
    </pre>
    <pre v-else-if="gameState.status == 'Battle'">
      {{ gameState.status }} stage. {{ gameState.currentPlayer }}'s turn to move.
    </pre>
    <pre v-else-if="gameState.status != null">
      {{ gameState.status[0] }} of {{ gameState.status[1] }}.
    </pre>

    <button @click="clipLink" style="" class="button">Copy competitor's invitation link</button><Transition name="fade"><span v-if="showTooltip">Copied!</span></Transition>
  </code>
</template>

<script setup>
  import { reactive, ref } from "vue";
  import GameBoard from "./GameBoard.vue";

  const props = defineProps({
    gameId: String,
    player: String,
  });

  const competitor = props.player == "White" ? "b" : "w";
  const wsUrl = `${window.location.protocol === "https:" ? "wss://" : "ws://"}${window.location.host}/ws/${props.gameId}`;
  const competitorLink = `${window.location.protocol}//${window.location.host}/?uid=${props.gameId}&p=${competitor}`;

  let gameState = reactive({
    board: [],
    currentPlayer: null,
    status: null,
  });

  var websocket = new WebSocket(wsUrl);
  websocket.onopen = logEvent;
  websocket.onclose = logEvent;
  websocket.onerror = logEvent;
  websocket.onmessage = update;

  function logEvent(event) {
    console.log(event);
  }

  function update(event) {
    let game = JSON.parse(event.data);

    gameState.currentPlayer = game.currentPlayer;
    gameState.status = game.status;

    const board = game.board.map((arr) => {
      let [[column, row], piece] = arr;

      return {
        column: column,
        row: row,
        rank: piece.rank,
        player: piece.player,
        id: piece.id,
      };
    });

    const diff = diffBoards(gameState.board, board);

    diff.toAdd.forEach((piece) => gameState.board.push(piece));

    diff.toUpdate.forEach((piece) => {
      const index = gameState.board.findIndex((p) => p.id == piece.id);
      gameState.board.splice(index, 1, piece);
    });

    diff.toDelete.forEach((piece) => {
      const index = gameState.board.findIndex((p) => p.id == piece.id);
      gameState.board.splice(index, 1);
    });

    console.log(gameState);
  }

  function diffBoards(prev, current) {
    let toAdd = current.filter((piece) => !prev.some((p) => p.id == piece.id));
    let toUpdate = current.filter((piece) => prev.some((p) => p.id == piece.id && (p.column != piece.column || p.row != piece.row)));
    let toDelete = prev.filter((piece) => !current.some((p) => p.id == piece.id));

    return {
      toAdd: toAdd,
      toUpdate: toUpdate,
      toDelete: toDelete,
    };
  }

  function move(from, to) {
    const message = JSON.stringify(["Move", { from: from, to: to }]);

    websocket.send(message);
  }

  function arrange(to) {
    const message = JSON.stringify(["Arrange", to]);

    websocket.send(message);
  }

  let showTooltip = ref(false);
  async function clipLink() {
    await navigator.clipboard.writeText(competitorLink);
    showTooltip.value = true;
    setTimeout(function () {
      showTooltip.value = false;
    }, 1000);
  }
</script>

<style scoped>
  pre {
    white-space: pre-line;
  }

  .fade-leave-active {
    transition: opacity 0.5s ease;
  }

  .fade-leave-to {
    opacity: 0;
  }
</style>