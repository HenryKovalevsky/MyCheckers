<template>
  <GameBoard :player="player" :board="gameState.board" @act="session.act" />

  <code style="display: flex; flex-direction: column; align-items: center">
    <pre>{{ getStatus(gameState) }}</pre>
    <button @click="clipLink" style="" class="button">Copy competitor's invitation link</button>
    <Transition name="fade"><span v-if="showTooltip">Copied!</span></Transition>
  </code>
</template>

<script setup>
  import GameBoard from "./GameBoard.vue";
  import Consts from "../core/Constants.js";

  import { reactive, ref } from "vue";
  import { initGameSession } from "../core/GameSession.js";
  import { buildLink, buildWsUrl } from "../core/Helpers.js";

  const props = defineProps({
    gameId: String,
    player: String,
  });

  const competitor = props.player == Consts.player.WHITE ? Consts.player.BLACK : Consts.player.WHITE;

  const wsUrl = buildWsUrl(props.gameId);
  const competitorLink = buildLink(props.gameId, competitor);

  let gameState = reactive({
    board: [],
    currentPlayer: null,
    status: null,
  });

  const session = initGameSession(props.player, wsUrl, gameState);

  const getStatus = (state) => {
    const capitalize = (str) => str.charAt(0).toUpperCase() + str.slice(1);

    switch (state.status) {
      case null:
        break;

      case Consts.status.DRAFT:
        return `Draft stage. ${capitalize(state.currentPlayer)}'s turn to set piece.`;

      case Consts.status.BATTLE:
        return `Battle stage. ${capitalize(state.currentPlayer)}'s turn to move.`;

      case Consts.status.DRAW:
        return `Draw.`;

      default:
        const [[status, player]] = Object.entries(state.status);
        return `${capitalize(status)} of ${capitalize(player)}.`;
    }
  };

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