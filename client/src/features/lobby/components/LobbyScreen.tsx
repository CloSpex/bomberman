import React, { useState } from "react";

interface LobbyScreenProps {
  playerName: string;
  setPlayerName: (name: string) => void;
  roomId: string;
  setRoomId: (id: string) => void;
  errorMessage: string;
  isCreatingRoom: boolean;
  createRoom: (gameMode: string) => void;
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
  const [selectedGameMode, setSelectedGameMode] = useState("standard");

  const gameModes = [
    {
      id: "standard",
      name: "Standard Mode",
      description: "Balanced gameplay with normal mechanics",
      color: "bg-blue-600 hover:bg-blue-700",
      features: [
        "Normal power-ups",
        "Standard bombs",
        "30% power-up drop rate",
      ],
    },
    {
      id: "chaos",
      name: "Chaos Mode",
      description: "Unpredictable gameplay with random elements",
      color: "bg-purple-600 hover:bg-purple-700",
      features: [
        "Random power-ups",
        "Mixed bomb types",
        "50% power-up drop rate",
        "Longer explosions",
      ],
    },
    {
      id: "speed",
      name: "Speed Mode",
      description: "Fast-paced gameplay with enhanced mechanics",
      color: "bg-green-600 hover:bg-green-700",
      features: [
        "Enhanced bombs (+1 range)",
        "Quick explosions",
        "40% power-up drop rate",
      ],
    },
  ];

  return (
    <div className="min-h-screen bg-gray-900 text-white p-8">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-4xl font-bold text-center mb-8 text-orange-500">
          Bomberman
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

          <div className="space-y-6">
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
              <h3 className="text-xl font-medium mb-4">Create New Room</h3>

              <div className="mb-4">
                <label className="block text-sm font-medium mb-3">
                  Select Game Mode
                </label>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                  {gameModes.map((mode) => (
                    <div
                      key={mode.id}
                      onClick={() => setSelectedGameMode(mode.id)}
                      className={`p-4 rounded-lg border-2 cursor-pointer transition-all ${
                        selectedGameMode === mode.id
                          ? "border-orange-500 bg-gray-700"
                          : "border-gray-600 bg-gray-750 hover:border-gray-500"
                      }`}
                    >
                      <div className="flex items-center justify-between mb-2">
                        <h4 className="font-semibold text-lg">{mode.name}</h4>
                        {selectedGameMode === mode.id && (
                          <div className="w-4 h-4 rounded-full bg-orange-500"></div>
                        )}
                      </div>
                      <p className="text-sm text-gray-400 mb-2">
                        {mode.description}
                      </p>
                      <ul className="text-xs text-gray-400 space-y-1">
                        {mode.features.map((feature, idx) => (
                          <li key={idx}>• {feature}</li>
                        ))}
                      </ul>
                    </div>
                  ))}
                </div>
              </div>

              <button
                onClick={() => createRoom(selectedGameMode)}
                disabled={!playerName.trim() || isCreatingRoom}
                className="w-full bg-green-600 hover:bg-green-700 disabled:bg-gray-600 text-white font-bold py-3 px-4 rounded transition-colors"
              >
                {isCreatingRoom
                  ? "Creating..."
                  : `Create ${
                      gameModes.find((m) => m.id === selectedGameMode)?.name
                    } Room`}
              </button>
            </div>

            <div className="border-t border-gray-600 pt-4">
              <h3 className="text-xl font-medium mb-3">Join Existing Room</h3>
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
                  className="w-full bg-orange-600 hover:bg-orange-700 disabled:bg-gray-600 text-white font-bold py-3 px-4 rounded transition-colors"
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
