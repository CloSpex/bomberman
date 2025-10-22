import React from "react";
import { Player } from "@interfaces/player.interface";

interface PlayerStatsProps {
  player: Player;
}

const PlayerStats: React.FC<PlayerStatsProps> = ({ player }) => {
  return (
    <div className="bg-gray-800 p-4 rounded-lg">
      <h3 className="text-lg font-semibold mb-3">Your Stats</h3>
      <div className="space-y-2 text-sm">
        <div>Bombs: {player.bombCount}</div>
        <div>Range: {player.bombRange}</div>
        <div>Speed: {player.speed}</div>
        <div>Status: {player.isAlive ? "Alive" : "Dead"}</div>
      </div>
    </div>
  );
};

export default PlayerStats;
