<template>
  <div class="board-container">
    <div :key="size" ref="board" class="board" @click="proceed">
      <div class="piece" v-for="item in gameState.pieces" :key="item.id" :style="{ transform: getPosition(item) }" :class="[{ selected: isSelected(item) }, getPieceForm(item)]"></div>
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

  let size = ref(0);
  let board = ref(null);
  let selected = ref(null);

  function getPosition(item) {
    return `translate(${(item.x * size.value) / boardSize}px, ${(item.y * size.value) / boardSize}px)`;
  }

  function getPieceForm(item) {
    return `${item.rank}-${item.player}`.toLowerCase();
  }

  function isSelected(item) {
    return selected.value != null && selected.value.id == item.id;
  }

  function proceed(event) {
    if (props.player != props.gameState.currentPlayer) return;

    let rect = event.target.getBoundingClientRect();
    let x = Math.floor((event.clientX - rect.left) / (rect.width / boardSize));
    let y = Math.floor((event.clientY - rect.top) / (rect.height / boardSize));

    if (props.gameState.status == "Draft") {
      emit("arrange", { x: x, y: y });
    } else {
      if (selected.value != null) {
        emit("move", selected.value, { x: x, y: y });

        selected.value = null;
      } else {
        let piece = props.gameState.pieces.find((p) => p.player == props.player && p.x == x && p.y == y);
        selected.value = piece;
      }
    }
    console.log({ x, y });
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