import type { PlayerSymbol } from '../../protocol/types';

export class TicTacToeState {
  board: PlayerSymbol[] = Array<PlayerSymbol>(9).fill('None');
  currentPlayer: Exclude<PlayerSymbol, 'None'> = 'X';
  winner: PlayerSymbol = 'None';
  winningCells: number[] = [];
  isDraw = false;

  get isGameOver(): boolean {
    return this.winner !== 'None' || this.isDraw;
  }

  reset(): void {
    this.board = Array<PlayerSymbol>(9).fill('None');
    this.currentPlayer = 'X';
    this.winner = 'None';
    this.winningCells = [];
    this.isDraw = false;
  }
}
