export type PlayerSymbol = 'X' | 'O' | 'None';

export type RoomStatus = 'waiting' | 'playing' | 'finished';

export type ClientMessageType = 'join_room' | 'make_move' | 'rejoin_room';

export type ServerMessageType =
  | 'room_joined'
  | 'game_state'
  | 'game_over'
  | 'player_left'
  | 'error';

export type ErrorCode =
  | 'INVALID_MESSAGE'
  | 'INVALID_PAYLOAD'
  | 'ROOM_NOT_FOUND'
  | 'ROOM_FULL'
  | 'NOT_IN_ROOM'
  | 'NOT_YOUR_TURN'
  | 'INVALID_MOVE'
  | 'GAME_OVER'
  | 'SESSION_INVALID';

export interface MessageEnvelope<TType extends string, TPayload> {
  type: TType;
  payload: TPayload;
}

export interface JoinRoomPayload {
  roomId?: string;
}

export interface MakeMovePayload {
  cellIndex: number;
}

export interface RejoinRoomPayload {
  roomId: string;
  sessionToken: string;
}

export type ClientMessage =
  | MessageEnvelope<'join_room', JoinRoomPayload>
  | MessageEnvelope<'make_move', MakeMovePayload>
  | MessageEnvelope<'rejoin_room', RejoinRoomPayload>;

export interface RoomJoinedPayload {
  roomId: string;
  yourPlayer: Exclude<PlayerSymbol, 'None'>;
  sessionToken: string;
  playerCount: number;
}

export interface GameStatePayload {
  roomId: string;
  status: RoomStatus;
  board: PlayerSymbol[];
  currentPlayer: Exclude<PlayerSymbol, 'None'>;
  yourPlayer: Exclude<PlayerSymbol, 'None'>;
  winner: PlayerSymbol;
  winningCells: number[];
  isDraw: boolean;
}

export interface GameOverPayload {
  roomId: string;
  winner: PlayerSymbol;
  winningCells: number[];
  isDraw: boolean;
}

export interface PlayerLeftPayload {
  roomId: string;
  leftPlayer: Exclude<PlayerSymbol, 'None'>;
}

export interface ErrorPayload {
  code: ErrorCode;
  message: string;
}

export type ServerMessage =
  | MessageEnvelope<'room_joined', RoomJoinedPayload>
  | MessageEnvelope<'game_state', GameStatePayload>
  | MessageEnvelope<'game_over', GameOverPayload>
  | MessageEnvelope<'player_left', PlayerLeftPayload>
  | MessageEnvelope<'error', ErrorPayload>;

export function createServerMessage<T extends ServerMessage['type']>(
  type: T,
  payload: Extract<ServerMessage, { type: T }>['payload'],
): Extract<ServerMessage, { type: T }> {
  return { type, payload } as Extract<ServerMessage, { type: T }>;
}
