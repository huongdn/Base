import type { ClientMessage, ClientMessageType, ErrorCode } from './types';

const CLIENT_MESSAGE_TYPES: ClientMessageType[] = [
  'join_room',
  'make_move',
  'rejoin_room',
];

export interface ParseSuccess {
  ok: true;
  message: ClientMessage;
}

export interface ParseFailure {
  ok: false;
  code: ErrorCode;
  message: string;
}

export type ParseResult = ParseSuccess | ParseFailure;

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function isNonEmptyString(value: unknown): value is string {
  return typeof value === 'string' && value.length > 0;
}

function isCellIndex(value: unknown): value is number {
  return typeof value === 'number' && Number.isInteger(value) && value >= 0 && value <= 8;
}

function parseJoinRoom(payload: unknown): ClientMessage | null {
  if (!isRecord(payload)) return null;
  if (payload.roomId !== undefined && typeof payload.roomId !== 'string') return null;
  return { type: 'join_room', payload: { roomId: payload.roomId as string | undefined } };
}

function parseMakeMove(payload: unknown): ClientMessage | null {
  if (!isRecord(payload)) return null;
  if (!isCellIndex(payload.cellIndex)) return null;
  return { type: 'make_move', payload: { cellIndex: payload.cellIndex } };
}

function parseRejoinRoom(payload: unknown): ClientMessage | null {
  if (!isRecord(payload)) return null;
  if (!isNonEmptyString(payload.roomId)) return null;
  if (!isNonEmptyString(payload.sessionToken)) return null;
  return {
    type: 'rejoin_room',
    payload: {
      roomId: payload.roomId,
      sessionToken: payload.sessionToken,
    },
  };
}

export function parseClientMessage(raw: string): ParseResult {
  let parsed: unknown;

  try {
    parsed = JSON.parse(raw);
  } catch {
    return {
      ok: false,
      code: 'INVALID_MESSAGE',
      message: 'Message must be valid JSON.',
    };
  }

  if (!isRecord(parsed) || typeof parsed.type !== 'string') {
    return {
      ok: false,
      code: 'INVALID_MESSAGE',
      message: 'Message must include a string "type" field.',
    };
  }

  if (!CLIENT_MESSAGE_TYPES.includes(parsed.type as ClientMessageType)) {
    return {
      ok: false,
      code: 'INVALID_MESSAGE',
      message: `Unknown message type: ${parsed.type}`,
    };
  }

  if (!isRecord(parsed.payload)) {
    return {
      ok: false,
      code: 'INVALID_PAYLOAD',
      message: 'Message must include a "payload" object.',
    };
  }

  let message: ClientMessage | null = null;

  switch (parsed.type) {
    case 'join_room':
      message = parseJoinRoom(parsed.payload);
      break;
    case 'make_move':
      message = parseMakeMove(parsed.payload);
      break;
    case 'rejoin_room':
      message = parseRejoinRoom(parsed.payload);
      break;
  }

  if (!message) {
    return {
      ok: false,
      code: 'INVALID_PAYLOAD',
      message: `Invalid payload for message type: ${parsed.type}`,
    };
  }

  return { ok: true, message };
}
