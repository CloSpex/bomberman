import React from "react";
import { Player } from "@interfaces/player.interface";

interface PlayersListProps {
  players: Player[];
  currentPlayerId: string;
}

const PlayersList: React.FC<PlayersListProps> = ({
  players,
  currentPlayerId,
}) => {
  const safePlayersList = Array.isArray(players) ? players : [];

  console.log("PlayersList - players prop:", players);
  console.log("PlayersList - safe list length:", safePlayersList.length);
  console.log("PlayersList - current player ID:", currentPlayerId);

  return (
    <div className="bg-gray-800 p-4 rounded-lg">
      <h3 className="text-lg font-semibold mb-3">
        Players ({safePlayersList.length}/4)
      </h3>

      {safePlayersList.length === 0 ? (
        <div className="text-gray-400 text-center py-4">
          <p className="text-sm">Waiting for players to join...</p>
        </div>
      ) : (
        <div className="space-y-2">
          {safePlayersList.map((player, index) => {
            if (!player) {
              console.warn(`Player at index ${index} is null/undefined`);
              return null;
            }

            const playerId = player.id || `player-${index}`;
            const playerName = player.name || "Unknown Player";
            const playerColor = player.color || "#888888";
            const isAlive = player.isAlive !== false;
            const isCurrentPlayer = playerId === currentPlayerId;

            return (
              <div
                key={playerId}
                className={`p-2 rounded flex items-center justify-between ${
                  isCurrentPlayer
                    ? "bg-gray-700 border border-orange-500"
                    : "bg-gray-700"
                }`}
              >
                <div className="flex items-center">
                  <div
                    className="w-4 h-4 rounded mr-2"
                    style={{ backgroundColor: playerColor }}
                  ></div>
                  <span
                    className={`${
                      !isAlive ? "line-through text-gray-500" : ""
                    }`}
                  >
                    {playerName}
                    {isCurrentPlayer && (
                      <span className="ml-2 text-xs text-orange-500">
                        (You)
                      </span>
                    )}
                  </span>
                </div>
                {!isAlive && <span className="text-red-500 text-sm">Dead</span>}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};

export default PlayersList;
