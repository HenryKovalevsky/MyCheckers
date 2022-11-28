<template>
  <div class="board-container" :style="{ width: `${state.size}px`, height: `${state.size}px` }">
    <div class="board" @click="proceed">
      <div class="piece" v-for="item in gameState.pieces" :key="item.id" :style="{ transform: getPosition(item) }" :class="[{ selected: isSelected(item) }, getPieceForm(item)]"></div>
    </div>
  </div>
</template>

<script setup>
  import { reactive } from "vue";

  const props = defineProps({
    gameState: Object,
    player: String,
  });
  const emit = defineEmits(["move", "arrange"]);

  const state = reactive({
    size: 600,
    selected: null,
  });
  const square = state.size / 8;

  function getPosition(item) {
    return `translate(${item.x * square}px, ${item.y * square}px)`;
  }

  function getPieceForm(item) {
    return `${item.rank}-${item.player}`.toLowerCase();
  }

  function isSelected(item) {
    return state.selected != null && state.selected.id == item.id;
  }

  function proceed(event) {
    if (props.player != props.gameState.currentPlayer) return;

    let rect = event.target.getBoundingClientRect();
    let x = Math.floor((event.clientX - rect.left) / square);
    let y = Math.floor((event.clientY - rect.top) / square);

    if (props.gameState.status == "Draft") {
      emit("arrange", { x: x, y: y });
    } else {
      if (state.selected != null) {
        emit("move", state.selected, { x: x, y: y });

        state.selected = null;
      } else {
        let piece = props.gameState.pieces.find((p) => p.player == props.player && p.x == x && p.y == y);
        state.selected = piece;
      }
    }
    console.log({ x, y });
  }
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