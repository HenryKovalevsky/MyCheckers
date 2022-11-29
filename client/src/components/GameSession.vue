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
  const link = `${window.location.protocol}//${window.location.host}/?uid=${props.gameId}&p=${competitor}`;

  let gameState = reactive({
    pieces: [],
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

  const columns = ["A", "B", "C", "D", "E", "F", "G", "H"];
  const rows = ["One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight"];

  function update(event) {
    let game = JSON.parse(event.data);

    gameState.pieces = game.board.map((arr) => {
      let [column, row] = arr[0];
      let piece = arr[1];
      return {
        x: columns.findIndex((i) => i == column),
        y: rows.findIndex((i) => i == row),
        rank: piece.rank,
        player: piece.player,
        id: piece.id,
      };
    });

    gameState.currentPlayer = game.currentPlayer;
    gameState.status = game.status;

    console.log(gameState);
  }

  function move(from, to) {
    const message = JSON.stringify([
      "Move",
      {
        from: [columns[from.x], rows[from.y]],
        to: [columns[to.x], rows[to.y]],
      },
    ]);

    websocket.send(message);

    // const piece = pieces.find((p) => p.id == id);
    // const index = pieces.findIndex((p) => p.id == id);
    // pieces.splice(index, 1, Object.assign(piece, coords));
    // console.log(pieces);
  }

  function arrange(to) {
    const message = JSON.stringify(["Arrange", [columns[to.x], rows[to.y]]]);

    websocket.send(message);
  }

  let showTooltip = ref(false);
  async function clipLink() {
    await navigator.clipboard.writeText(link);
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