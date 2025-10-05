import { GameState } from "common/enums/gameState.enum";
import { Player } from "./player.interface";
import { GameBoard } from "./gameBoard.interface";

export interface GameRoom {
  id: string;
  players: Player[];
  state: GameState;
  board: GameBoard;
  lastUpdate: string;
}