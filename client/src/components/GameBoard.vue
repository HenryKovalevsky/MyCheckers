<template>
  <div class="board-container">
    <div class="board" :key="size" @click="act" ref="boardRef">
      <!-- prettier-ignore -->
      <GamePiece v-for="piece in board" 
          :key="piece.id" 
          :style="{ transform: getPosition(piece) }" 
          :piece="piece"></GamePiece>
    </div>
  </div>
</template>

<script setup>
  import GamePiece from "./GamePiece.vue";

  import { ref, onMounted, nextTick } from "vue";

  const props = defineProps({
    board: Object
  });
  const emit = defineEmits(["act"]);

  let size = ref(0);
  let boardRef = ref(null);

  onMounted(() => {
    nextTick(() => {
      window.addEventListener("resize", onResize);
      onResize();
    });
  });

  function onResize() {
    let rect = boardRef.value.getBoundingClientRect();
    size.value = rect.width;
  }

  const boardSize = 8;

  function getPosition(piece) {
    const px = (piece.x * size.value) / boardSize;
    const py = (piece.y * size.value) / boardSize;
    return `translate(${px}px, ${py}px)`;
  }

  function act(event) {
    let rect = event.target.getBoundingClientRect();
    let x = Math.floor((event.clientX - rect.left) / (rect.width / boardSize));
    let y = Math.floor((event.clientY - rect.top) / (rect.height / boardSize));

    emit("act", { x, y });
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
</style>