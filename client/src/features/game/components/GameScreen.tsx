import React from "react";
import * as signalR from "@microsoft/signalr";

import { GameRoom } from "@interfaces/gameRoom.interface";
import { GameState } from "@enums/gameState.enum";
import GameCanvas from "./GameCanvas";
import PlayersList from "./PlayersList";
import PlayerStats from "./PlayerStats";
import GameControls from "./GameControls";
import WinnerModal from "../modals/WinnerModal";
import useKeyboardControls from "../hooks/useKeyboardControls";

interface GameScreenProps {
  gameRoom: GameRoom;
  connection: signalR.HubConnection | null;
  roomId: string;
  currentPlayerId: string;
  startGame: () => void;
}

const GameScreen: React.FC<GameScreenProps> = ({
  gameRoom,
  connection,
  roomId,
  currentPlayerId,
  startGame,
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

  const getCurrentPlayer = () => {
    return gameRoom?.players.find((p) => p.id === currentPlayerId);
  };

  const getWinner = () => {
    if (gameRoom.state !== GameState.Finished) return null;
    return gameRoom.players.find((p) => p.isAlive);
  };

  return (
    <div className="min-h-screen bg-gray-900 text-white p-4">
      <div className="max-w-6xl mx-auto">
        <h1 className="text-3xl font-bold text-center mb-4 text-orange-500">
          Bomberman
        </h1>
        <h2>Room code is {roomId}</h2>
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

              <GameCanvas gameRoom={gameRoom} />

              {gameRoom.state === GameState.Playing && (
                <div className="mt-4 text-center text-sm text-gray-300">
                  Use WASD or Arrow keys to move, Space or Enter to place bomb
                </div>
              )}
            </div>
          </div>

          <div className="space-y-4">
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
