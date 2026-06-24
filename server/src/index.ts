import { createWebSocketServer } from './core';

const PORT = process.env.PORT || 3000;

createWebSocketServer(Number(PORT));

console.log(`WebSocket server running on port ${PORT}`);
