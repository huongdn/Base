import { WebSocket } from 'ws';
import type { ClientMessage } from '../protocol';
import { generateRoomId, Room } from './Room';
import { sendError } from './messaging';

export class RoomManager {
  private readonly rooms = new Map<string, Room>();
  private readonly socketRoom = new WeakMap<WebSocket, string>();

  joinRoom(ws: WebSocket, roomId?: string): void {
    let room: Room | undefined;

    if (roomId) {
      room = this.rooms.get(roomId.toUpperCase());
      if (!room) {
        sendError(ws, 'ROOM_NOT_FOUND', `Room "${roomId}" does not exist.`);
        return;
      }
    } else {
      const newId = this.createUniqueRoomId();
      room = new Room(newId);
      this.rooms.set(newId, room);
    }

    const result = room.join(ws);
    if (!result) {
      sendError(ws, 'ROOM_FULL', 'This room already has two players.');
      return;
    }

    this.socketRoom.set(ws, room.id);
    room.sendJoined(ws, result.yourPlayer, result.sessionToken);
    room.broadcastState();
  }

  rejoinRoom(ws: WebSocket, roomId: string, sessionToken: string): void {
    const room = this.rooms.get(roomId.toUpperCase());
    if (!room) {
      sendError(ws, 'ROOM_NOT_FOUND', `Room "${roomId}" does not exist.`);
      return;
    }

    const player = room.rejoin(ws, sessionToken);
    if (!player) {
      sendError(ws, 'SESSION_INVALID', 'Session token is not valid for this room.');
      return;
    }

    this.socketRoom.set(ws, room.id);
    room.sendJoined(ws, player, sessionToken);
    room.broadcastState();
  }

  makeMove(ws: WebSocket, cellIndex: number): void {
    const room = this.getRoomForSocket(ws);
    if (!room) {
      sendError(ws, 'NOT_IN_ROOM', 'Join a room before making a move.');
      return;
    }

    const player = room.findPlayerBySocket(ws);
    if (!player) {
      sendError(ws, 'NOT_IN_ROOM', 'You are not seated in this room.');
      return;
    }

    const result = room.makeMove(player, cellIndex);
    if (!result.ok) {
      sendError(ws, result.code, result.message);
      return;
    }

    room.broadcastState();

    if (room.status === 'finished') {
      room.broadcastGameOver();
    }
  }

  handleDisconnect(ws: WebSocket): void {
    const room = this.getRoomForSocket(ws);
    if (!room) return;

    const leftPlayer = room.disconnect(ws);
    this.socketRoom.delete(ws);

    if (leftPlayer && room.status !== 'finished') {
      room.notifyPlayerLeft(leftPlayer);
    }

    if (room.isEmpty()) {
      this.rooms.delete(room.id);
    }
  }

  handleMessage(ws: WebSocket, message: ClientMessage): void {
    switch (message.type) {
      case 'join_room':
        this.joinRoom(ws, message.payload.roomId);
        break;
      case 'rejoin_room':
        this.rejoinRoom(ws, message.payload.roomId, message.payload.sessionToken);
        break;
      case 'make_move':
        this.makeMove(ws, message.payload.cellIndex);
        break;
    }
  }

  private getRoomForSocket(ws: WebSocket): Room | undefined {
    const roomId = this.socketRoom.get(ws);
    if (!roomId) return undefined;
    return this.rooms.get(roomId);
  }

  private createUniqueRoomId(): string {
    let id = generateRoomId();
    while (this.rooms.has(id)) {
      id = generateRoomId();
    }
    return id;
  }
}
