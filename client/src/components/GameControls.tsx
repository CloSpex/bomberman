import React from "react";

const GameControls: React.FC = () => {
  return (
    <div className="bg-gray-800 p-4 rounded-lg">
      <h3 className="text-lg font-semibold mb-3">Controls</h3>
      <div className="text-sm space-y-1">
        <div> WASD / Arrow Keys: Move</div>
        <div> Space / Enter: Place Bomb</div>
        <div> Collect power-ups to upgrade</div>
      </div>
    </div>
  );
};

export default GameControls;
