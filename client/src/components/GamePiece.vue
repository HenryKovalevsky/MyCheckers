<template>
  <div :class="['piece', getPieceIcon(piece), { selected: piece.isSelected }]"></div>
</template>

<script setup>
  const props = defineProps({
    piece: Object
  });

  const getPieceIcon = (piece) => `${piece.rank}-${piece.player}`.toLowerCase();
</script>

<style>
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