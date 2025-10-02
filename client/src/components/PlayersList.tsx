import React from "react";
import { Player } from "../types";

interface PlayersListProps {
  players: Player[];
  currentPlayerId: string;
}

const PlayersList: React.FC<PlayersListProps> = ({
  players,
  currentPlayerId,
}) => {
  return (
    <div className="bg-gray-800 p-4 rounded-lg">
      <h3 className="text-lg font-semibold mb-3">
        Players ({players.length}/4)
      </h3>
      <div className="space-y-2">
        {players.map((player) => (
          <div
            key={player.id}
            className={`p-2 rounded flex items-center justify-between ${
              player.id === currentPlayerId
                ? "bg-gray-700 border border-orange-500"
                : "bg-gray-700"
            }`}
          >
            <div className="flex items-center">
              <div
                className="w-4 h-4 rounded mr-2"
                style={{ backgroundColor: player.color }}
              ></div>
              <span
                className={`${
                  !player.isAlive ? "line-through text-gray-500" : ""
                }`}
              >
                {player.name}
              </span>
            </div>
            {!player.isAlive && (
              <span className="text-red-500 text-sm">💀</span>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default PlayersList;
