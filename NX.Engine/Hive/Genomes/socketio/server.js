/**
 * 
 * A Socket.IO server
 * 
 */

// Get express.js
const express = require('express');
// Make the app
const app = express();
// Make the server 
const server = require('http').createServer(app);
// Add socket.io
const io = require('socket.io').listen(server);

// The active connections
const connections = [];
// Listen
io.sockets.on('connection', (socket) => {
    // Add connection
    connections.push(socket);
    // Tell world
    //console.log('Connection added: %s connections', connections.length);
    // Setup for disconnect
    socket.on('disconnect', () => {
        // Remove connection
        connections.splice(connections.indexOf(socket), 1);
        // Tell world
        //console.log('Connection removed: %s connections', connections.length);
    });

    // Handle our message
    socket.on('nxsio', (message) => {
        // Tell world
        console.log('Message : ', message);
        // Push out
        //io.sockets.emit('nxsio', message);
    });
});

// Run server
server.listen(3000);
console.log('Socket.IO server is running');