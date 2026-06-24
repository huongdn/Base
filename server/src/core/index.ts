import { WebSocketServer } from 'ws';
import { attachWebSocketHandlers } from './connection';
import { RoomManager } from './RoomManager';

export function createWebSocketServer(port: number): WebSocketServer {
  const roomManager = new RoomManager();
  const wss = new WebSocketServer({ port });

  wss.on('connection', (ws) => {
    console.log('Client connected');
    attachWebSocketHandlers(ws, roomManager);
  });

  return wss;
}
