import React from "react";
import { Player } from "common/interfaces/player.interface";

interface WinnerModalProps {
  winner: Player | null | undefined;
}

const WinnerModal: React.FC<WinnerModalProps> = ({ winner }) => {
  return (
    <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50">
      <div className="bg-gray-800 p-8 rounded-lg text-center max-w-md">
        <h2 className="text-3xl font-bold mb-4 text-orange-500">Game Over!</h2>
        {winner ? (
          <>
            <div
              className="w-20 h-20 mx-auto mb-4 rounded"
              style={{ backgroundColor: winner.color }}
            ></div>
            <p className="text-2xl mb-2">{winner.name} Wins!</p>
            <p className="text-gray-400">Congratulations on your victory!</p>
          </>
        ) : (
          <p className="text-xl text-gray-400">No winners - It's a draw!</p>
        )}
        <button
          onClick={() => window.location.reload()}
          className="mt-6 bg-orange-600 hover:bg-orange-700 text-white px-6 py-3 rounded font-semibold"
        >
          Play Again
        </button>
      </div>
    </div>
  );
};

export default WinnerModal;
