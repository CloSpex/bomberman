import { useState, useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import LobbyScreen from "../lobby/components/LobbyScreen";
import GameScreen from "./components/GameScreen";
import LoadingScreen from "@sharedComponents/LoadingScreen";
import { GameRoom } from "@interfaces/gameRoom.interface";
import { GameState } from "common/enums/gameState.enum";

const normalizeGameRoom = (data: any): GameRoom | null => {
  if (!data) {
    console.error("Received null/undefined data");
    return null;
  }

  console.log("Normalizing room data:", data);

  try {
    let parsedState = GameState.Waiting;
    const stateValue = data.state || data.State;
    if (typeof stateValue === "string") {
      parsedState =
        GameState[stateValue as keyof typeof GameState] ?? GameState.Waiting;
    } else if (typeof stateValue === "number") {
      parsedState = stateValue;
    }

    const normalized: GameRoom = {
      id: data.id || data.Id || "",
      players: Array.isArray(data.players)
        ? data.players
        : Array.isArray(data.Players)
        ? data.Players
        : [],
      state: parsedState,
      board: {
        width: data.board?.width || data.Board?.Width || 15,
        height: data.board?.height || data.Board?.Height || 13,
        grid: data.board?.grid || data.Board?.Grid || [],
        bombs: Array.isArray(data.board?.bombs)
          ? data.board.bombs
          : Array.isArray(data.Board?.Bombs)
          ? data.Board.Bombs
          : [],
        explosions: Array.isArray(data.board?.explosions)
          ? data.board.explosions
          : Array.isArray(data.Board?.Explosions)
          ? data.Board.Explosions
          : [],
        powerUps: Array.isArray(data.board?.powerUps)
          ? data.board.powerUps
          : Array.isArray(data.Board?.PowerUps)
          ? data.Board.PowerUps
          : [],
      },
      lastUpdate:
        data.lastUpdate || data.LastUpdate || new Date().toISOString(),
      textView: data.textView || data.TextView || "",
    };

    if (!normalized.id) {
      console.error("Room ID is missing");
      return null;
    }

    if (!Array.isArray(normalized.players)) {
      console.error("Players is not an array:", normalized.players);
      normalized.players = [];
    }

    console.log("Normalized room data:", normalized);
    console.log(
      "Normalized state:",
      normalized.state,
      "Type:",
      typeof normalized.state,
    );
    console.log("Players count:", normalized.players.length);

    return normalized;
  } catch (error) {
    console.error("Error normalizing room data:", error);
    return null;
  }
};

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
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
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
          console.log("Connection ID:", connection.connectionId);

          const handleRoomUpdate = (eventName: string) => (roomData: any) => {
            console.log(`${eventName} received:`, roomData);
            console.log("Raw data type:", typeof roomData);
            console.log(
              "Players in data:",
              roomData?.players || roomData?.Players,
            );

            const normalizedRoom = normalizeGameRoom(roomData);

            if (normalizedRoom) {
              console.log(
                `Setting game room from ${eventName}:`,
                normalizedRoom,
              );
              setGameRoom(normalizedRoom);

              if (roomData.rendererType || roomData.RendererType) {
                setRendererType(roomData.rendererType || roomData.RendererType);
              }

              const theme = roomData.theme || roomData.Theme;
              const bombFactory = roomData.bombFactory || roomData.BombFactory;
              if (theme) console.log("Theme:", theme);
              if (bombFactory) console.log("BombFactory:", bombFactory);
            } else {
              console.error(`Failed to normalize room data from ${eventName}`);
            }
          };

          connection.on("PlayerJoined", handleRoomUpdate("PlayerJoined"));
          connection.on("GameStarted", handleRoomUpdate("GameStarted"));
          connection.on("GameUpdated", handleRoomUpdate("GameUpdated"));
          connection.on("RendererChanged", handleRoomUpdate("RendererChanged"));
          connection.on("FactoryChanged", handleRoomUpdate("FactoryChanged"));
          connection.on("ThemeChanged", handleRoomUpdate("ThemeChanged"));

          connection.on("JoinFailed", (message: string) => {
            console.error("Join failed:", message);
            setErrorMessage(message);
            setGameRoom(null);
          });

          connection.on("StartFailed", (message: string) => {
            console.error("Start failed:", message);
            setErrorMessage(message);
          });

          connection.onreconnecting((error) => {
            console.warn("Connection lost, reconnecting...", error);
            setIsConnected(false);
          });

          connection.onreconnected((connectionId) => {
            console.log("Reconnected with ID:", connectionId);
            setIsConnected(true);
            setCurrentPlayerId(connectionId || "");
          });

          connection.onclose((error) => {
            console.error("Connection closed:", error);
            setIsConnected(false);
            setErrorMessage("Connection to server lost");
          });
        })
        .catch((error) => {
          console.error("Error connecting to SignalR hub:", error);
          setErrorMessage("Failed to connect to game server");
          setIsConnected(false);
        });

      return () => {
        if (connection.state === signalR.HubConnectionState.Connected) {
          connection.stop();
        }
      };
    }
  }, [connection]);

  const createRoom = async () => {
    if (!connection || !playerName.trim()) {
      setErrorMessage("Please enter a player name");
      return;
    }

    if (connection.state !== signalR.HubConnectionState.Connected) {
      setErrorMessage("Not connected to server");
      return;
    }

    try {
      setIsCreatingRoom(true);
      setErrorMessage("");

      const newRoomId = Math.random()
        .toString(36)
        .substring(2, 8)
        .toUpperCase();

      setRoomId(newRoomId);
      console.log("Creating room with ID:", newRoomId);
      console.log("Player name:", playerName);

      await connection.invoke("JoinRoom", newRoomId, playerName);
      setCurrentPlayerId(connection.connectionId || "");

      console.log("Room creation request sent successfully");
    } catch (error) {
      console.error("Error creating room:", error);
      setErrorMessage(`Failed to create room: ${error}`);
      setGameRoom(null);
    } finally {
      setIsCreatingRoom(false);
    }
  };

  const joinRoom = async () => {
    if (!connection || !playerName.trim() || !roomId.trim()) {
      setErrorMessage("Please enter both player name and room ID");
      return;
    }

    if (connection.state !== signalR.HubConnectionState.Connected) {
      setErrorMessage("Not connected to server");
      return;
    }

    try {
      setErrorMessage("");
      console.log("Joining room:", roomId, "as", playerName);

      await connection.invoke("JoinRoom", roomId, playerName);
      setCurrentPlayerId(connection.connectionId || "");

      console.log("Room join request sent successfully");
    } catch (error) {
      console.error("Error joining room:", error);
      setErrorMessage(`Failed to join room: ${error}`);
      setGameRoom(null);
    }
  };

  const startGame = async () => {
    if (!connection || !roomId) {
      console.error("Cannot start game: no connection or room ID");
      return;
    }

    if (connection.state !== signalR.HubConnectionState.Connected) {
      setErrorMessage("Not connected to server");
      return;
    }

    try {
      console.log("Starting game in room:", roomId);
      await connection.invoke("StartGame", roomId);
    } catch (error) {
      console.error("Error starting game:", error);
      setErrorMessage(`Failed to start game: ${error}`);
    }
  };

  const changeRenderer = async (newRenderer: string) => {
    if (!connection || !roomId) {
      console.error("Cannot change renderer: no connection or room ID");
      return;
    }

    if (connection.state !== signalR.HubConnectionState.Connected) {
      setErrorMessage("Not connected to server");
      return;
    }

    try {
      console.log(`Requesting renderer change to ${newRenderer}`);
      await connection.invoke("ChangeRenderer", roomId, newRenderer);
    } catch (error) {
      console.error("Error changing renderer:", error);
      setErrorMessage(`Failed to change renderer: ${error}`);
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
      rendererType={rendererType}
      changeRenderer={changeRenderer}
    />
  );
}
