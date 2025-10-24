import React, { useRef, useEffect, useCallback } from "react";

import { CELL_SIZE, BOARD_WIDTH, BOARD_HEIGHT } from "../boardInfo";
import { GameRoom } from "@interfaces/gameRoom.interface";
import { CellType } from "@enums/cellType.enum";
import { PowerUpType } from "@enums/powerUpType.enum";

interface GameCanvasProps {
  gameRoom: GameRoom & { theme?: string };
}

const THEMES = {
  classic: {
    background: "#2a2a2a",
    empty: "#90EE90",
    wall: "#444444",
    wallStroke: "#666666",
    destructible: "#8B4513",
    destructibleStroke: "#A0522D",
    explosion: "#FF4500",
    explosionCenter: "#FFFF00",
    bomb: "#000000",
    bombStroke: "#333333",
    powerUpBomb: "#FFD700",
    powerUpRange: "#FF69B4",
    powerUpSpeed: "#00BFFF",
    powerUpStroke: "#000000",
  },
  neon: {
    background: "#0a0a1a",
    empty: "#1a1a2e",
    wall: "#0f3460",
    wallStroke: "#16213e",
    destructible: "#533483",
    destructibleStroke: "#6b4984",
    explosion: "#00fff5",
    explosionCenter: "#ff00ff",
    bomb: "#ff00ff",
    bombStroke: "#00fff5",
    powerUpBomb: "#00fff5",
    powerUpRange: "#ff00ff",
    powerUpSpeed: "#ffff00",
    powerUpStroke: "#00fff5",
  },
};

const GameCanvas: React.FC<GameCanvasProps> = ({ gameRoom }) => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const animationFrameRef = useRef<number | null>(null);

  const getTheme = () => {
    const themeName = gameRoom.theme?.toLowerCase() || "classic";
    return THEMES[themeName as keyof typeof THEMES] || THEMES.classic;
  };

  const renderGame = useCallback(() => {
    const canvas = canvasRef.current;
    if (!canvas || !gameRoom) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    const theme = getTheme();

    ctx.fillStyle = theme.background;
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    for (let y = 0; y < BOARD_HEIGHT; y++) {
      for (let x = 0; x < BOARD_WIDTH; x++) {
        const cellType = gameRoom.board.grid[y]?.[x] ?? CellType.Empty;
        const pixelX = x * CELL_SIZE;
        const pixelY = y * CELL_SIZE;

        switch (cellType) {
          case CellType.Wall:
            ctx.fillStyle = theme.wall;
            ctx.fillRect(pixelX, pixelY, CELL_SIZE, CELL_SIZE);
            ctx.strokeStyle = theme.wallStroke;
            ctx.strokeRect(pixelX, pixelY, CELL_SIZE, CELL_SIZE);
            break;
          case CellType.DestructibleWall:
            ctx.fillStyle = theme.destructible;
            ctx.fillRect(pixelX, pixelY, CELL_SIZE, CELL_SIZE);
            ctx.strokeStyle = theme.destructibleStroke;
            ctx.strokeRect(pixelX, pixelY, CELL_SIZE, CELL_SIZE);
            break;
          default:
            ctx.fillStyle = theme.empty;
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
          ? theme.powerUpBomb
          : powerUp.type === PowerUpType.RangeUp
          ? theme.powerUpRange
          : theme.powerUpSpeed;
      ctx.fillRect(pixelX, pixelY, size, size);
      ctx.strokeStyle = theme.powerUpStroke;
      ctx.strokeRect(pixelX, pixelY, size, size);
    });

    gameRoom.board.explosions?.forEach((explosion) => {
      const pixelX = explosion.x * CELL_SIZE;
      const pixelY = explosion.y * CELL_SIZE;

      ctx.fillStyle = theme.explosion;
      ctx.fillRect(pixelX, pixelY, CELL_SIZE, CELL_SIZE);

      ctx.fillStyle = theme.explosionCenter;
      ctx.fillRect(pixelX + 4, pixelY + 4, CELL_SIZE - 8, CELL_SIZE - 8);
    });

    gameRoom.board.bombs?.forEach((bomb) => {
      const pixelX = bomb.x * CELL_SIZE + 4;
      const pixelY = bomb.y * CELL_SIZE + 4;
      const size = CELL_SIZE - 8;

      ctx.fillStyle = theme.bomb;
      ctx.fillRect(pixelX, pixelY, size, size);
      ctx.strokeStyle = theme.bombStroke;
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
      className="border border-gray-600 mx-auto block"
      style={{ backgroundColor: getTheme().background }}
    />
  );
};

export default GameCanvas;
