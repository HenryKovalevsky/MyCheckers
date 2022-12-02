<template>
  <div class="board-container">
    <div class="board" :key="size" @click="proceed" ref="board">
      <!-- prettier-ignore -->
      <div class="piece" v-for="piece in gameState.board" 
          :key="piece.id" 
          :style="{ transform: getPosition(piece) }" 
          :class="[{ selected: isSelected(piece) }, getPieceIcon(piece)]"></div>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, nextTick } from "vue";

  const props = defineProps({
    gameState: Object,
    player: String,
  });
  const emit = defineEmits(["move", "arrange"]);

  const boardSize = 8;
  const columns = ["A", "B", "C", "D", "E", "F", "G", "H"];
  const rows = ["One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight"];

  let size = ref(0);
  let board = ref(null);
  let selected = ref(null);

  function getPosition(piece) {
    const x = columns.findIndex((i) => i == piece.column);
    const y = rows.findIndex((i) => i == piece.row);
    return `translate(${(x * size.value) / boardSize}px, ${(y * size.value) / boardSize}px)`;
  }

  function getPieceIcon(piece) {
    return `${piece.rank}-${piece.player}`.toLowerCase();
  }

  function isSelected(piece) {
    return selected.value != null && selected.value.id == piece.id;
  }

  function proceed(event) {
    if (props.player != props.gameState.currentPlayer) return;

    let rect = event.target.getBoundingClientRect();
    let x = Math.floor((event.clientX - rect.left) / (rect.width / boardSize));
    let y = Math.floor((event.clientY - rect.top) / (rect.height / boardSize));

    switch (props.gameState.status) {
      case "Draft":
        emit("arrange", [columns[x], rows[y]]);
        break;

      case "Battle":
        if (selected.value != null) {
          const from = [selected.value.column, selected.value.row];
          const to = [columns[x], rows[y]];

          emit("move", from, to);

          selected.value = null;
        } else {
          let piece = props.gameState.board.find((p) => p.player == props.player && p.column == columns[x] && p.row == rows[y]);
          selected.value = piece;
        }
        break;
    }
  }

  function onResize() {
    let rect = board.value.getBoundingClientRect();
    size.value = rect.width;
  }

  onMounted(() => {
    nextTick(() => {
      window.addEventListener("resize", onResize);
      onResize();
    });
  });
</script>

<style>
  .board-container {
    position: relative;
    display: block;
    left: 0;
    right: 0;
    margin-top: 140px;
    margin-left: auto;
    margin-right: auto;

    max-width: 600px;
    width: 90%;
    aspect-ratio: 1 / 1;
    max-height: 600px;
  }

  .board {
    position: absolute;
    top: 0;
    left: 0;
    height: 100%;
    width: 100%;
    cursor: pointer;
    background-image: url("../assets/board.svg");
  }

  @media (max-width: 600px) {
    .board {
      cursor: auto;
    }
  }

  .piece {
    position: absolute;
    top: 0;
    left: 0;
    width: 12.5%;
    height: 12.5%;
    background-size: cover;
    z-index: 2;
    pointer-events: none;
    will-change: transform;
    transition: transform 0.3s;
  }

  .man-white {
    background-image: url("../assets/man-white.svg");
  }

  .man-black {
    background-image: url("../assets/man-black.svg");
  }

  .king-white {
    background-image: url("../assets/king-white.svg");
  }

  .king-black {
    background-image: url("../assets/king-black.svg");
  }

  .selected {
    background-color: rgba(20, 85, 30, 0.5);
  }
</style>