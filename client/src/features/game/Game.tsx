import { useState, useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import LobbyScreen from "../lobby/components/LobbyScreen";
import GameScreen from "./components/GameScreen";
import LoadingScreen from "@sharedComponents/LoadingScreen";
import { GameRoom } from "@interfaces/gameRoom.interface";

export default function BombermanGame() {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(
    null,
  );
  const [gameRoom, setGameRoom] = useState<GameRoom | null>(null);
  const [playerName, setPlayerName] = useState("");
  const [roomId, setRoomId] = useState("");
  const [isConnected, setIsConnected] = useState(false);
  const [currentPlayerId, setCurrentPlayerId] = useState<string>("");
  const [errorMessage, setErrorMessage] = useState("");
  const [isCreatingRoom, setIsCreatingRoom] = useState(false);
  const [rendererType, setRendererType] = useState("canvas");

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

          connection.on("PlayerJoined", (roomData: any) => {
            console.log("PlayerJoined received:", roomData);
            console.log(
              "Theme:",
              roomData.theme,
              "BombFactory:",
              roomData.bombFactory,
            );
            setGameRoom(roomData);
            if (roomData.rendererType) {
              setRendererType(roomData.rendererType);
            }
          });

          connection.on("GameStarted", (roomData: any) => {
            console.log("GameStarted received:", roomData);
            console.log(
              "Theme:",
              roomData.theme,
              "BombFactory:",
              roomData.bombFactory,
            );
            setGameRoom(roomData);
            if (roomData.rendererType) {
              setRendererType(roomData.rendererType);
            }
          });

          connection.on("GameUpdated", (roomData: any) => {
            console.log("GameUpdated received:", roomData);
            console.log(
              "Theme:",
              roomData.theme,
              "BombFactory:",
              roomData.bombFactory,
            );
            setGameRoom(roomData);
            if (roomData.rendererType) {
              setRendererType(roomData.rendererType);
            }
          });

          connection.on("RendererChanged", (roomData: any) => {
            console.log("RendererChanged received:", roomData);
            console.log(
              "Theme:",
              roomData.theme,
              "BombFactory:",
              roomData.bombFactory,
            );
            setGameRoom(roomData);
            if (roomData.rendererType) {
              setRendererType(roomData.rendererType);
            }
          });

          connection.on("FactoryChanged", (roomData: any) => {
            console.log("FactoryChanged received:", roomData);
            console.log(
              "Theme:",
              roomData.theme,
              "BombFactory:",
              roomData.bombFactory,
            );
            setGameRoom(roomData);
          });

          connection.on("ThemeChanged", (roomData: any) => {
            console.log("ThemeChanged received:", roomData);
            console.log(
              "Theme:",
              roomData.theme,
              "BombFactory:",
              roomData.bombFactory,
            );
            setGameRoom(roomData);
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

  const changeRenderer = async (newRenderer: string) => {
    if (!connection || !roomId) return;
    try {
      console.log(`Requesting renderer change to ${newRenderer}`);
      await connection.invoke("ChangeRenderer", roomId, newRenderer);
    } catch (error) {
      console.error("Error changing renderer:", error);
    }
  };

  if (!isConnected) return <LoadingScreen />;

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
      rendererType={rendererType}
      changeRenderer={changeRenderer}
    />
  );
}
