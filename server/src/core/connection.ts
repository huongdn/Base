import { WebSocket } from 'ws';
import { parseClientMessage } from '../protocol';
import { RoomManager } from './RoomManager';
import { sendError } from './messaging';

export function attachWebSocketHandlers(ws: WebSocket, roomManager: RoomManager): void {
  ws.on('message', (data) => {
    const raw = data.toString();
    const parsed = parseClientMessage(raw);

    if (!parsed.ok) {
      sendError(ws, parsed.code, parsed.message);
      return;
    }

    roomManager.handleMessage(ws, parsed.message);
  });

  ws.on('close', () => {
    roomManager.handleDisconnect(ws);
  });
}
