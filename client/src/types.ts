export interface Player {
  id: string;
  name: string;
  x: number;
  y: number;
  isAlive: boolean;
  bombCount: number;
  bombRange: number;
  color: string;
}

export interface Bomb {
  id: string;
  x: number;
  y: number;
  playerId: string;
  placedAt: string;
  range: number;
}

export interface Explosion {
  x: number;
  y: number;
  createdAt: string;
}

export interface PowerUp {
  x: number;
  y: number;
  type: PowerUpType;
}

export interface GameBoard {
  width: number;
  height: number;
  grid: number[][];
  bombs: Bomb[];
  explosions: Explosion[];
  powerUps: PowerUp[];
}

export interface GameRoom {
  id: string;
  players: Player[];
  state: GameState;
  board: GameBoard;
  lastUpdate: string;
}

export enum CellType {
  Empty = 0,
  Wall = 1,
  DestructibleWall = 2,
}

export enum GameState {
  Waiting = 0,
  Playing = 1,
  Finished = 2,
}

export enum PowerUpType {
  BombUp = 0,
  RangeUp = 1,
  SpeedUp = 2,
}

export const CELL_SIZE = 32;
export const BOARD_WIDTH = 15;
export const BOARD_HEIGHT = 13;
