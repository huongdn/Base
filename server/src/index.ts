import { WebSocketServer, WebSocket } from 'ws';

const PORT = process.env.PORT || 3000;

const wss = new WebSocketServer({ port: Number(PORT) });

console.log(`WebSocket server running on port ${PORT}`);

wss.on('connection', (ws: WebSocket) => {
  console.log('Client connected');

  ws.on('message', (data) => {
    console.log('Received:', data.toString());
    ws.send(`Echo: ${data}`);
  });

  ws.on('close', () => {
    console.log('Client disconnected');
  });
});