<template>
  <header class="header"><a href="/" class="logo">Henry's checkers</a></header>

  <section v-if="!gameId" class="intro">
    <button v-if="!links.whitePlayerLink" @click="createGame" class="button" role="button">CREATE A GAME</button>

    <section v-if="links.whitePlayerLink" class="links">
      <a :href="links.whitePlayerLink" class="button">Join as White</a>
      <div class="vl"></div>
      <a :href="links.blackPlayerLink" class="button">Join as Black</a>
    </section>

    <article class="rules">
      <h2>The rules of this variant of checkers are:</h2>

      <p><b>Board.</b> Played on an 8×8 board with alternating dark and light squares.</p>
      <p><b>Starting position.</b> Each player starts with 10 pieces that must be arranged on the three rows closest to their own side. The row closest to each player is called the "crownhead" or "kings row". Usually, the colors of the pieces are black and white. The player with white pieces (lighter color) moves first.</p>
      <p><b>Pieces.</b> There are two kinds of pieces: "men" and "kings". Kings are marked with an additional sign in the center of the piece.</p>
      <p><b>Men.</b> Men move forward, forward diagonally, move left or right to an adjacent unoccupied square.</p>
      <p><b>Kings.</b> If a player's piece moves into the kings row on the opposing player's side of the board and stops there, that piece is to be "crowned", becoming a "king" and gaining the ability to move backwards as well as forwards and the move distance increases twice.</p>
      <p><b>Capture.</b> If the adjacent square contains an opponent's piece, and the square immediately beyond it is vacant, the opponent's piece may be captured (and removed from the game) by jumping over it. Jumping can be done forward and backward. Multiple-jump moves are possible if, when the jumping piece lands, there is another piece that can be jumped. Jumping is mandatory and cannot be passed up to make a non-jumping move. When there is more than one way for a player to jump, one may choose which sequence to make, not necessarily the sequence that will result in the most captures. However, one must make all the captures in that sequence.</p>
      <p><b>Winning and draws.</b> A player with no pieces left loses. A game is a draw if neither opponent has the possibility to win the game. The game is considered a draw when the same position repeats itself for the third time, with the same player having the move each time. If one player proposes a draw and his opponent accepts the offer. If during 15 moves both players moved only kings, without moving any men and without making any capture.</p>

      <p>P.S.: The rules can be changed in the future in a sake of game balance.</p>
    </article>
  </section>

  <GameSession v-else :gameId="gameId" :player="player"></GameSession>
</template>

<script setup>
  import { reactive } from "vue";
  import GameSession from "./components/GameSession.vue";

  const params = new Proxy(new URLSearchParams(window.location.search), {
    get: (searchParams, prop) => searchParams.get(prop),
  });

  let gameId = params.uid;
  const player = params.p == "w" ? "White" : "Black";

  let links = reactive({
    whitePlayerLink: null,
    blackPlayerLink: null,
  });

  async function createGame() {
    let respone = await fetch("/api/games", { method: "POST" });

    const gameId = await respone.text();
    const link = `${window.location.protocol}//${window.location.host}/?uid=${gameId}`;

    links.whitePlayerLink = link + "&p=w";
    links.blackPlayerLink = link + "&p=b";
  }
</script>

<style lang="scss">
  $background-color: #262421;
  $text-color: rgba(255, 255, 255, 0.87);
  $accent-color: rgba(184, 134, 230, 0.877);

  .header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    background-color: $background-color;
    border-bottom: 1px solid gray;
    height: 58px;
    width: 100%;
    position: fixed;
    z-index: 10;

    .logo {
      background-color: $background-color;
      padding: 9px 24px;
      text-transform: uppercase;
      text-decoration: none;
      color: $text-color;
      flex-grow: 0;
      border: 0;
      white-space: nowrap;
    }
  }

  .intro {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    margin-top: 120px;
  }

  .links {
    display: flex;
    justify-content: space-between;
    align-items: center;
    width: 400px;
  }

  .rules {
    margin-top: 60px;
    width: 1200px;
  }

  .button {
    align-items: center;
    background-color: $background-color;
    border: 1px solid $text-color;
    box-sizing: border-box;
    color: $text-color;
    cursor: pointer;
    display: inline-flex;
    fill: $text-color;
    font-family: Inter, sans-serif;
    font-size: 16px;
    font-weight: 500;
    height: 48px;
    justify-content: center;
    letter-spacing: -0.8px;
    line-height: 24px;
    min-width: 140px;
    outline: 0;
    padding: 0 17px;
    text-align: center;
    text-decoration: none;
    transition: all 0.3s;
    user-select: none;
    -webkit-user-select: none;
    touch-action: manipulation;

    &:focus {
      color: $accent-color;
    }

    &:hover {
      border-color: $accent-color;
      color: $accent-color;
      fill: $accent-color;
    }

    &:active {
      border-color: $accent-color;
      color: $accent-color;
      fill: $accent-color;
    }
  }

  .vl {
    border-left: 1px solid $accent-color;
    height: 32px;
  }

</style>