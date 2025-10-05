import { Bomb } from "./bomb.interface";
import { Explosion } from "./explosion.interface";
import { PowerUp } from "./powerUp.interface";

export interface GameBoard {
  width: number;
  height: number;
  grid: number[][];
  bombs: Bomb[];
  explosions: Explosion[];
  powerUps: PowerUp[];
}