import { randomBytes, randomUUID } from 'crypto';
import { WebSocket } from 'ws';
import { TicTacToeLogic, TicTacToeState } from '../games/tictactoe';
import {
  createServerMessage,
  type ErrorCode,
  type GameStatePayload,
  type PlayerSymbol,
  type RoomStatus,
} from '../protocol';
import { sendError, sendMessage } from './messaging';

const ROOM_CODE_CHARS = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
const ROOM_CODE_LENGTH = 6;

type ActivePlayer = Exclude<PlayerSymbol, 'None'>;

interface PlayerSlot {
  sessionToken: string;
  ws: WebSocket | null;
}

export class Room {
  readonly id: string;
  private readonly game = new TicTacToeState();
  private readonly logic = new TicTacToeLogic();
  private readonly slots: Record<ActivePlayer, PlayerSlot | null> = { X: null, O: null };

  constructor(id: string) {
    this.id = id;
  }

  get playerCount(): number {
    return (this.slots.X ? 1 : 0) + (this.slots.O ? 1 : 0);
  }

  get status(): RoomStatus {
    if (this.game.isGameOver) return 'finished';
    if (this.playerCount < 2) return 'waiting';
    return 'playing';
  }

  isEmpty(): boolean {
    const xConnected = this.slots.X?.ws !== null && this.slots.X?.ws !== undefined;
    const oConnected = this.slots.O?.ws !== null && this.slots.O?.ws !== undefined;
    return !xConnected && !oConnected;
  }

  join(ws: WebSocket): { yourPlayer: ActivePlayer; sessionToken: string; playerCount: number } | null {
    const existing = this.findSlotBySocket(ws);
    if (existing) {
      return {
        yourPlayer: existing,
        sessionToken: this.slots[existing]!.sessionToken,
        playerCount: this.playerCount,
      };
    }

    const symbol = this.nextOpenSymbol();
    if (!symbol) return null;

    const sessionToken = randomUUID();
    this.slots[symbol] = { sessionToken, ws };

    return {
      yourPlayer: symbol,
      sessionToken,
      playerCount: this.playerCount,
    };
  }

  rejoin(ws: WebSocket, sessionToken: string): ActivePlayer | null {
    for (const symbol of ['X', 'O'] as const) {
      const slot = this.slots[symbol];
      if (slot && slot.sessionToken === sessionToken) {
        slot.ws = ws;
        return symbol;
      }
    }
    return null;
  }

  findPlayerBySocket(ws: WebSocket): ActivePlayer | null {
    return this.findSlotBySocket(ws);
  }

  findPlayerBySessionToken(sessionToken: string): ActivePlayer | null {
    for (const symbol of ['X', 'O'] as const) {
      if (this.slots[symbol]?.sessionToken === sessionToken) return symbol;
    }
    return null;
  }

  makeMove(player: ActivePlayer, cellIndex: number): { ok: true } | { ok: false; code: ErrorCode; message: string } {
    if (this.status === 'waiting') {
      return { ok: false, code: 'NOT_IN_ROOM', message: 'Waiting for a second player.' };
    }

    if (this.status === 'finished') {
      return { ok: false, code: 'GAME_OVER', message: 'This game has already ended.' };
    }

    if (this.game.currentPlayer !== player) {
      return { ok: false, code: 'NOT_YOUR_TURN', message: 'It is not your turn.' };
    }

    if (this.game.board[cellIndex] !== 'None') {
      return { ok: false, code: 'INVALID_MOVE', message: 'That cell is already taken.' };
    }

    if (!this.logic.tryMakeMove(this.game, cellIndex)) {
      return { ok: false, code: 'INVALID_MOVE', message: 'That move is not allowed.' };
    }

    return { ok: true };
  }

  disconnect(ws: WebSocket): ActivePlayer | null {
    const player = this.findSlotBySocket(ws);
    if (!player) return null;

    const slot = this.slots[player];
    if (slot) slot.ws = null;
    return player;
  }

  broadcastState(): void {
    this.forEachConnectedPlayer((player, ws) => {
      sendMessage(ws, createServerMessage('game_state', this.buildGameStatePayload(player)));
    });
  }

  broadcastGameOver(): void {
    const payload = {
      roomId: this.id,
      winner: this.game.winner,
      winningCells: this.game.winningCells,
      isDraw: this.game.isDraw,
    };

    this.forEachConnectedPlayer((_player, ws) => {
      sendMessage(ws, createServerMessage('game_over', payload));
    });
  }

  notifyPlayerLeft(leftPlayer: ActivePlayer): void {
    const payload = { roomId: this.id, leftPlayer };
    this.forEachConnectedPlayer((_player, ws) => {
      sendMessage(ws, createServerMessage('player_left', payload));
    });
  }

  sendJoined(ws: WebSocket, yourPlayer: ActivePlayer, sessionToken: string): void {
    sendMessage(
      ws,
      createServerMessage('room_joined', {
        roomId: this.id,
        yourPlayer,
        sessionToken,
        playerCount: this.playerCount,
      }),
    );
  }

  private nextOpenSymbol(): ActivePlayer | null {
    if (!this.slots.X) return 'X';
    if (!this.slots.O) return 'O';
    return null;
  }

  private findSlotBySocket(ws: WebSocket): ActivePlayer | null {
    for (const symbol of ['X', 'O'] as const) {
      if (this.slots[symbol]?.ws === ws) return symbol;
    }
    return null;
  }

  private forEachConnectedPlayer(fn: (player: ActivePlayer, ws: WebSocket) => void): void {
    for (const symbol of ['X', 'O'] as const) {
      const ws = this.slots[symbol]?.ws;
      if (ws) fn(symbol, ws);
    }
  }

  private buildGameStatePayload(yourPlayer: ActivePlayer): GameStatePayload {
    return {
      roomId: this.id,
      status: this.status,
      board: [...this.game.board],
      currentPlayer: this.game.currentPlayer,
      yourPlayer,
      winner: this.game.winner,
      winningCells: [...this.game.winningCells],
      isDraw: this.game.isDraw,
    };
  }
}

export function generateRoomId(): string {
  const bytes = randomBytes(ROOM_CODE_LENGTH);
  let id = '';
  for (let i = 0; i < ROOM_CODE_LENGTH; i++) {
    id += ROOM_CODE_CHARS[bytes[i] % ROOM_CODE_CHARS.length];
  }
  return id;
}
