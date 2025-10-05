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