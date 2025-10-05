import { PowerUpType } from "common/enums/powerUpType.enum";

export interface PowerUp {
  x: number;
  y: number;
  type: PowerUpType;
}