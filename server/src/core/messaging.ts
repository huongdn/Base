import { WebSocket } from 'ws';
import { createServerMessage, type ErrorCode, type ServerMessage } from '../protocol';

export function sendMessage(ws: WebSocket, message: ServerMessage): void {
  if (ws.readyState === WebSocket.OPEN) {
    ws.send(JSON.stringify(message));
  }
}

export function sendError(ws: WebSocket, code: ErrorCode, message: string): void {
  sendMessage(ws, createServerMessage('error', { code, message }));
}
