import { useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import { GameRoom } from "@interfaces/gameRoom.interface";
import { GameState } from "@enums/gameState.enum";

export default function useKeyboardControls(
  connection: signalR.HubConnection | null,
  gameRoom: GameRoom | null,
  roomId: string,
  currentPlayerId: string,
) {
  const gameRoomRef = useRef(gameRoom);
  const playerIdRef = useRef(currentPlayerId);

  useEffect(() => {
    gameRoomRef.current = gameRoom;
    playerIdRef.current = currentPlayerId;
  }, [gameRoom, currentPlayerId]);

  useEffect(() => {
    const handleKeyPress = (event: KeyboardEvent) => {
      const room = gameRoomRef.current;
      const playerId = playerIdRef.current;

      if (!connection || !room || room.state !== GameState.Playing) return;

      const currentPlayer = room.players.find((p) => p.id === playerId);
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
  }, [connection, roomId]);
}
