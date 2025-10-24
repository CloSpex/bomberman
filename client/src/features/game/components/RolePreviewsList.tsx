import React from "react";
import { Player } from "@interfaces/player.interface";

interface RolePreviewsListProps {
  previews: Player[];
}

const RolePreviewsList: React.FC<RolePreviewsListProps> = ({ previews }) => {
  return (
    <div className="bg-gray-800 p-4 rounded-lg mt-4">
      <h3 className="text-lg font-semibold mb-2">Player Role Previews</h3>
      <div className="space-y-2">
        {previews.map((player) => (
          <div
            key={player.id}
            className="flex items-center justify-between p-2 bg-gray-700 rounded"
          >
            <div className="flex items-center">
              <div
                className="w-4 h-4 rounded mr-2"
                style={{ backgroundColor: player.color }}
              ></div>
              <span>{player.name}</span>
            </div>
            <span className="text-sm">
              Speed: {player.speed} | Bombs: {player.bombCount} | Range:{" "}
              {player.bombRange}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
};

export default RolePreviewsList;
