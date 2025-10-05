import { useState, useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import LobbyScreen from "../lobby/components/LobbyScreen";
import GameScreen from "./components/GameScreen";
import LoadingScreen from "@sharedComponents/LoadingScreen";
import { GameRoom } from "@interfaces/gameRoom.interface";

export default function BombermanGame() {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(
    null
  );
  const [gameRoom, setGameRoom] = useState<GameRoom | null>(null);
  const [playerName, setPlayerName] = useState("");
  const [roomId, setRoomId] = useState("");
  const [isConnected, setIsConnected] = useState(false);
  const [currentPlayerId, setCurrentPlayerId] = useState<string>("");
  const [errorMessage, setErrorMessage] = useState("");
  const [isCreatingRoom, setIsCreatingRoom] = useState(false);

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5288/gamehub")
      .build();

    setConnection(newConnection);
  }, []);

  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          setIsConnected(true);
          console.log("Connected to SignalR hub");

          connection.on("PlayerJoined", (room: GameRoom) => {
            setGameRoom(room);
          });

          connection.on("GameStarted", (room: GameRoom) => {
            setGameRoom(room);
          });

          connection.on("GameUpdated", (room: GameRoom) => {
            setGameRoom(room);
          });

          connection.on("JoinFailed", (message: string) => {
            setErrorMessage(message);
          });
        })
        .catch((error) => {
          console.error("Error connecting to SignalR hub:", error);
          setErrorMessage("Failed to connect to game server");
        });

      return () => {
        connection.stop();
      };
    }
  }, [connection]);

  const createRoom = async () => {
    if (!connection || !playerName.trim()) return;

    try {
      setIsCreatingRoom(true);
      const newRoomId = Math.random()
        .toString(36)
        .substring(2, 8)
        .toUpperCase();
      setRoomId(newRoomId);
      console.log("Creating room with ID:", newRoomId);
      await connection.invoke("JoinRoom", newRoomId, playerName);
      setCurrentPlayerId(connection.connectionId || "");
      setErrorMessage("");
    } catch (error) {
      console.error("Error creating room:", error);
      setErrorMessage("Failed to create room");
    } finally {
      setIsCreatingRoom(false);
    }
  };

  const joinRoom = async () => {
    if (!connection || !playerName.trim() || !roomId.trim()) return;

    try {
      await connection.invoke("JoinRoom", roomId, playerName);
      setCurrentPlayerId(connection.connectionId || "");
      setErrorMessage("");
    } catch (error) {
      console.error("Error joining room:", error);
      setErrorMessage("Failed to join room");
    }
  };

  const startGame = async () => {
    if (!connection || !roomId) return;

    try {
      await connection.invoke("StartGame", roomId);
    } catch (error) {
      console.error("Error starting game:", error);
    }
  };

  if (!isConnected) {
    return <LoadingScreen />;
  }

  if (!gameRoom) {
    return (
      <LobbyScreen
        playerName={playerName}
        setPlayerName={setPlayerName}
        roomId={roomId}
        setRoomId={setRoomId}
        errorMessage={errorMessage}
        isCreatingRoom={isCreatingRoom}
        createRoom={createRoom}
        joinRoom={joinRoom}
      />
    );
  }

  return (
    <GameScreen
      gameRoom={gameRoom}
      connection={connection}
      roomId={roomId}
      currentPlayerId={currentPlayerId}
      startGame={startGame}
    />
  );
};

