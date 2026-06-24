import type { PlayerSymbol } from '../../protocol/types';
import { TicTacToeState } from './GameState';

const WIN_CONDITIONS: readonly number[][] = [
  [0, 1, 2],
  [3, 4, 5],
  [6, 7, 8],
  [0, 3, 6],
  [1, 4, 7],
  [2, 5, 8],
  [0, 4, 8],
  [2, 4, 6],
];

export class TicTacToeLogic {
  tryMakeMove(state: TicTacToeState, cellIndex: number): boolean {
    if (state.isGameOver) return false;
    if (state.board[cellIndex] !== 'None') return false;

    state.board[cellIndex] = state.currentPlayer;

    const winningCells = findWinningCells(state.board, state.currentPlayer);
    if (winningCells) {
      state.winner = state.currentPlayer;
      state.winningCells = winningCells;
    } else if (isBoardFull(state.board)) {
      state.isDraw = true;
    } else {
      state.currentPlayer = state.currentPlayer === 'X' ? 'O' : 'X';
    }

    return true;
  }
}

function findWinningCells(
  board: PlayerSymbol[],
  player: Exclude<PlayerSymbol, 'None'>,
): number[] | null {
  for (const condition of WIN_CONDITIONS) {
    if (
      board[condition[0]] === player &&
      board[condition[1]] === player &&
      board[condition[2]] === player
    ) {
      return condition;
    }
  }
  return null;
}

function isBoardFull(board: PlayerSymbol[]): boolean {
  return board.every((cell) => cell !== 'None');
}
