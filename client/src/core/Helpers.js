import Consts from "./Constants.js"

export function getParams() {
  const params = new Proxy(new URLSearchParams(window.location.search), {
    get: (searchParams, prop) => searchParams.get(prop),
  });

  return {
    gameId: params.uid,
    player: params.p == "w" ? Consts.player.WHITE : Consts.player.BLACK
  };
}

export function buildLink(gameId, player) {
  const url = `${window.location.protocol}//${window.location.host}/`;
  const params = `?uid=${gameId}&p=${player.charAt(0)}`;

  return url + params;
}

export function buildWsUrl(gameId) {
  const protocol = window.location.protocol === "https:" ? "wss:" : "ws:";
  const url = `${protocol}//${window.location.host}/ws/${gameId}`;

  return url;
}