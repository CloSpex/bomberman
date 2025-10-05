import React, { useRef, useEffect, useCallback } from "react";

import { CELL_SIZE, BOARD_WIDTH, BOARD_HEIGHT } from "../boardInfo";
import { GameRoom } from "@interfaces/gameRoom.interface";
import { CellType } from "@enums/cellType.enum";
import { PowerUpType } from "@enums/powerUpType.enum";

interface GameCanvasProps {
  gameRoom: GameRoom;
}

const GameCanvas: React.FC<GameCanvasProps> = ({ gameRoom }) => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const animationFrameRef = useRef<number | null>(null);

  const renderGame = useCallback(() => {
    const canvas = canvasRef.current;
    if (!canvas || !gameRoom) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    ctx.fillStyle = "#2a2a2a";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    for (let y = 0; y < BOARD_HEIGHT; y++) {
      for (let x = 0; x < BOARD_WIDTH; x++) {
        const cellType = gameRoom.board.grid[y]?.[x] ?? CellType.Empty;
        const pixelX = x * CELL_SIZE;
        const pixelY = y * CELL_SIZE;

        switch (cellType) {
          case CellType.Wall:
            ctx.fillStyle = "#444444";
            ctx.fillRect(pixelX, pixelY, CELL_SIZE, CELL_SIZE);
            ctx.strokeStyle = "#666666";
            ctx.strokeRect(pixelX, pixelY, CELL_SIZE, CELL_SIZE);
            break;
          case CellType.DestructibleWall:
            ctx.fillStyle = "#8B4513";
            ctx.fillRect(pixelX, pixelY, CELL_SIZE, CELL_SIZE);
            ctx.strokeStyle = "#A0522D";
            ctx.strokeRect(pixelX, pixelY, CELL_SIZE, CELL_SIZE);
            break;
          default:
            ctx.fillStyle = "#90EE90";
            ctx.fillRect(pixelX, pixelY, CELL_SIZE, CELL_SIZE);
            break;
        }
      }
    }

    gameRoom.board.powerUps?.forEach((powerUp) => {
      const pixelX = powerUp.x * CELL_SIZE + CELL_SIZE / 4;
      const pixelY = powerUp.y * CELL_SIZE + CELL_SIZE / 4;
      const size = CELL_SIZE / 2;

      ctx.fillStyle =
        powerUp.type === PowerUpType.BombUp
          ? "#FFD700"
          : powerUp.type === PowerUpType.RangeUp
          ? "#FF69B4"
          : "#00BFFF";
      ctx.fillRect(pixelX, pixelY, size, size);
      ctx.strokeStyle = "#000000";
      ctx.strokeRect(pixelX, pixelY, size, size);
    });

    gameRoom.board.explosions?.forEach((explosion) => {
      const pixelX = explosion.x * CELL_SIZE;
      const pixelY = explosion.y * CELL_SIZE;

      ctx.fillStyle = "#FF4500";
      ctx.fillRect(pixelX, pixelY, CELL_SIZE, CELL_SIZE);

      ctx.fillStyle = "#FFFF00";
      ctx.fillRect(pixelX + 4, pixelY + 4, CELL_SIZE - 8, CELL_SIZE - 8);
    });

    gameRoom.board.bombs?.forEach((bomb) => {
      const pixelX = bomb.x * CELL_SIZE + 4;
      const pixelY = bomb.y * CELL_SIZE + 4;
      const size = CELL_SIZE - 8;

      ctx.fillStyle = "#000000";
      ctx.fillRect(pixelX, pixelY, size, size);
      ctx.strokeStyle = "#333333";
      ctx.strokeRect(pixelX, pixelY, size, size);
    });

    gameRoom.players.forEach((player) => {
      if (!player.isAlive) return;

      const pixelX = player.x * CELL_SIZE + 2;
      const pixelY = player.y * CELL_SIZE + 2;
      const size = CELL_SIZE - 4;

      ctx.fillStyle = player.color;
      ctx.fillRect(pixelX, pixelY, size, size);
      ctx.strokeStyle = "#000000";
      ctx.strokeRect(pixelX, pixelY, size, size);

      ctx.fillStyle = "#FFFFFF";
      ctx.font = "12px Arial";
      ctx.textAlign = "center";
      ctx.fillText(player.name, pixelX + size / 2, pixelY - 4);
    });

    animationFrameRef.current = requestAnimationFrame(renderGame);
  }, [gameRoom]);

  useEffect(() => {
    renderGame();
    return () => {
      if (animationFrameRef.current) {
        cancelAnimationFrame(animationFrameRef.current);
      }
    };
  }, [gameRoom, renderGame]);

  return (
    <canvas
      ref={canvasRef}
      width={BOARD_WIDTH * CELL_SIZE}
      height={BOARD_HEIGHT * CELL_SIZE}
      className="border border-gray-600 bg-green-200 mx-auto block"
    />
  );
};

export default GameCanvas;
