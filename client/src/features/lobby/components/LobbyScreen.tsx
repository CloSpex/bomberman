import React from "react";

interface LobbyScreenProps {
  playerName: string;
  setPlayerName: (name: string) => void;
  roomId: string;
  setRoomId: (id: string) => void;
  errorMessage: string;
  isCreatingRoom: boolean;
  createRoom: () => void;
  joinRoom: () => void;
}

const LobbyScreen: React.FC<LobbyScreenProps> = ({
  playerName,
  setPlayerName,
  roomId,
  setRoomId,
  errorMessage,
  isCreatingRoom,
  createRoom,
  joinRoom,
}) => {
  return (
    <div className="min-h-screen bg-gray-900 text-white p-8">
      <div className="max-w-md mx-auto">
        <h1 className="text-4xl font-bold text-center mb-8 text-orange-500">
          🎮 Bomberman
        </h1>

        <div className="bg-gray-800 p-6 rounded-lg shadow-lg">
          <h2 className="text-2xl font-semibold mb-6 text-center">
            Join or Create Game
          </h2>

          {errorMessage && (
            <div className="bg-red-600 text-white p-3 rounded mb-4">
              {errorMessage}
            </div>
          )}

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2">
                Player Name
              </label>
              <input
                type="text"
                value={playerName}
                onChange={(e) => setPlayerName(e.target.value)}
                className="w-full px-3 py-2 bg-gray-700 rounded border border-gray-600 focus:border-orange-500 focus:outline-none"
                placeholder="Enter your name"
                maxLength={20}
              />
            </div>

            <div className="border-t border-gray-600 pt-4">
              <h3 className="text-lg font-medium mb-3">Create New Room</h3>
              <button
                onClick={createRoom}
                disabled={!playerName.trim() || isCreatingRoom}
                className="w-full bg-green-600 hover:bg-green-700 disabled:bg-gray-600 text-white font-bold py-2 px-4 rounded transition-colors mb-4"
              >
                {isCreatingRoom ? "Creating..." : "Create New Room"}
              </button>
            </div>

            <div className="border-t border-gray-600 pt-4">
              <h3 className="text-lg font-medium mb-3">Join Existing Room</h3>
              <div className="space-y-3">
                <input
                  type="text"
                  value={roomId}
                  onChange={(e) => setRoomId(e.target.value.toUpperCase())}
                  className="w-full px-3 py-2 bg-gray-700 rounded border border-gray-600 focus:border-orange-500 focus:outline-none"
                  placeholder="Enter room ID (e.g. ABC123)"
                  maxLength={6}
                />
                <button
                  onClick={joinRoom}
                  disabled={!playerName.trim() || !roomId.trim()}
                  className="w-full bg-orange-600 hover:bg-orange-700 disabled:bg-gray-600 text-white font-bold py-2 px-4 rounded transition-colors"
                >
                  Join Room
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LobbyScreen;
