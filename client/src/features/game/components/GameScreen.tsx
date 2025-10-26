import React, { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { GameRoom } from "@interfaces/gameRoom.interface";
import { GameState } from "@enums/gameState.enum";
import GameCanvas from "./GameCanvas";
import PlayersList from "./PlayersList";
import PlayerStats from "./PlayerStats";
import GameControls from "./GameControls";
import WinnerModal from "../modals/WinnerModal";
import useKeyboardControls from "../hooks/useKeyboardControls";
import RolePreviewsList from "./RolePreviewsList";
import { Player } from "@interfaces/player.interface";

interface GameScreenProps {
  gameRoom: GameRoom;
  connection: signalR.HubConnection | null;
  roomId: string;
  currentPlayerId: string;
  startGame: () => void;
  rendererType: string;
  changeRenderer: (newRenderer: string) => void;
  gameMode: string;
  gameModeDescription: string;
}

const GameScreen: React.FC<GameScreenProps> = ({
  gameRoom,
  connection,
  roomId,
  currentPlayerId,
  startGame,
  rendererType,
  changeRenderer,
  gameMode,
  gameModeDescription,
}) => {
  useKeyboardControls(connection, gameRoom, roomId, currentPlayerId);

  const getGameStateText = () => {
    switch (gameRoom?.state) {
      case GameState.Waiting:
        return "Waiting for players";
      case GameState.Playing:
        return "Game in progress";
      case GameState.Finished:
        return "Game finished";
      default:
        return "Unknown state";
    }
  };

  const [rolePreviews, setRolePreviews] = useState<Player[]>([]);

  const getCurrentPlayer = () => {
    return gameRoom?.players.find((p) => p.id === currentPlayerId);
  };

  const getWinner = () => {
    if (gameRoom.state !== GameState.Finished) return null;
    return gameRoom.players.find((p) => p.isAlive);
  };

  const fetchRolePreviews = async () => {
    if (!connection) return;

    try {
      await connection.invoke("RolePreviews");
    } catch (error) {
      console.error("Failed to fetch role previews:", error);
    }
  };

  useEffect(() => {
    if (!connection) return;

    const handleRolePreviews = (previews: Player[]) => {
      setRolePreviews(previews);
    };

    connection.on("RolePreviews", handleRolePreviews);

    return () => {
      connection.off("RolePreviews", handleRolePreviews);
    };
  }, [connection]);

  const getGameModeColor = () => {
    switch (gameMode.toLowerCase()) {
      case "chaos":
        return "bg-purple-600";
      case "speed":
        return "bg-green-600";
      case "power":
        return "bg-red-600";
      default:
        return "bg-blue-600";
    }
  };

  return (
    <div className="min-h-screen bg-gray-900 text-white p-4">
      <div className="max-w-6xl mx-auto">
        <h1 className="text-3xl font-bold text-center mb-2 text-orange-500">
          Bomberman
        </h1>

        <div className="flex justify-center items-center mb-2 space-x-2">
          <span
            className={`px-4 py-1 rounded-full text-sm font-semibold ${getGameModeColor()}`}
          >
            {gameMode.toUpperCase()} MODE
          </span>
        </div>

        {gameModeDescription && (
          <div className="text-center text-sm text-gray-400 mb-2">
            {gameModeDescription}
          </div>
        )}

        <h2 className="text-center mb-4">
          Room code:{" "}
          <span className="font-mono bg-gray-800 px-3 py-1 rounded">
            {roomId}
          </span>
        </h2>

        <div className="flex justify-center mb-4 space-x-2">
          {["canvas", "json", "text"].map((type) => (
            <button
              key={type}
              onClick={() => changeRenderer(type)}
              className={`px-4 py-2 rounded font-semibold transition-colors ${
                rendererType === type
                  ? "bg-orange-600 hover:bg-orange-700"
                  : "bg-gray-700 hover:bg-gray-600"
              }`}
            >
              {type.toUpperCase()}
            </button>
          ))}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          <div className="lg:col-span-3">
            <div className="bg-gray-800 p-4 rounded-lg">
              <div className="mb-4 text-center">
                <span className="text-lg font-semibold">
                  {getGameStateText()}
                </span>
                {gameRoom.state === GameState.Waiting &&
                  gameRoom.players.length >= 2 && (
                    <button
                      onClick={startGame}
                      className="ml-4 bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded"
                    >
                      Start Game
                    </button>
                  )}
              </div>

              {rendererType === "canvas" && (
                <div>
                  <GameCanvas gameRoom={gameRoom} />
                </div>
              )}

              {rendererType === "json" && (
                <div className="bg-black rounded p-4 overflow-auto max-h-[600px]">
                  <pre className="text-green-400 text-sm">
                    {JSON.stringify(gameRoom, null, 2)}
                  </pre>
                </div>
              )}

              {rendererType === "text" && (
                <div className="bg-black rounded p-4 overflow-auto max-h-[600px]">
                  <pre className="text-yellow-300 text-sm whitespace-pre-wrap">
                    {(gameRoom as any).textView ??
                      "Waiting for server text render..."}
                  </pre>
                </div>
              )}

              {gameRoom.state === GameState.Playing && (
                <div className="mt-4 text-center text-sm text-gray-300">
                  Use WASD or Arrow keys to move, Space or Enter to place bomb
                </div>
              )}
            </div>
          </div>

          <div className="space-y-4">
            <div className="mt-4">
              <button
                onClick={fetchRolePreviews}
                className="bg-blue-600 hover:bg-blue-700 px-4 py-2 rounded w-full"
              >
                Load Role Previews
              </button>

              <RolePreviewsList previews={rolePreviews} />
            </div>

            <PlayersList
              players={gameRoom.players}
              currentPlayerId={currentPlayerId}
            />
            {gameRoom.state === GameState.Playing && getCurrentPlayer() && (
              <PlayerStats player={getCurrentPlayer()!} />
            )}
            <GameControls />
          </div>
        </div>
      </div>

      {gameRoom.state === GameState.Finished && (
        <WinnerModal winner={getWinner()} />
      )}
    </div>
  );
};

export default GameScreen;
