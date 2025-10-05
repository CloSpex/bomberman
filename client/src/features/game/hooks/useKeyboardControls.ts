import { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import { Player } from "@interfaces/player.interface";
import { GameRoom } from "@interfaces/gameRoom.interface";
import { GameState } from "@enums/gameState.enum";

const useKeyboardControls = (
  connection: signalR.HubConnection | null,
  gameRoom: GameRoom | null,
  roomId: string,
  currentPlayerId: string
) => {
  useEffect(() => {
    const handleKeyPress = (event: KeyboardEvent) => {
      if (!connection || !gameRoom || gameRoom.state !== GameState.Playing)
        return;

      const currentPlayer = gameRoom.players.find(
        (p: Player) => p.id === currentPlayerId
      );
      if (!currentPlayer || !currentPlayer.isAlive) return;

      let deltaX = 0;
      let deltaY = 0;

      switch (event.key.toLowerCase()) {
        case "arrowup":
        case "w":
          deltaY = -1;
          break;
        case "arrowdown":
        case "s":
          deltaY = 1;
          break;
        case "arrowleft":
        case "a":
          deltaX = -1;
          break;
        case "arrowright":
        case "d":
          deltaX = 1;
          break;
        case " ":
        case "enter":
          event.preventDefault();
          connection.invoke("PlaceBomb", roomId);
          return;
      }

      if (deltaX !== 0 || deltaY !== 0) {
        event.preventDefault();
        connection.invoke("MovePlayer", roomId, deltaX, deltaY);
      }
    };

    window.addEventListener("keydown", handleKeyPress);
    return () => window.removeEventListener("keydown", handleKeyPress);
  }, [connection, gameRoom, roomId, currentPlayerId]);
};

export default useKeyboardControls;
