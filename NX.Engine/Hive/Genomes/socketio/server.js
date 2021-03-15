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

// from environment
var raw = process.env.CHANNEL;
// Use default if needed
if (!raw) raw = 'nxsio';
// Break up
let channels = raw.split(',');
if (!Array.isArray(channels)) channels = [channels];

// Listen
io.sockets.on('connection', (socket) => {

    // Add connection
    connections.push(socket);
    // Setup for disconnect
    socket.on('disconnect', () => {
        // Remove connection
        connections.splice(connections.indexOf(socket), 1);
    });

    // Do each channel
    if (channels.length > 0) {
        // Setup internal channel
        socket.on(channels[0], (message) => {
            // Push out
            io.sockets.emit(channels[0], message);
        });
    }

    if (channels.length > 1) {
        // Setup internal channel
        socket.on(channels[1], (message) => {
            // Push out
            io.sockets.emit(channels[1], message);
        });
    }
    
    if (channels.length > 2) {
        // Setup internal channel
        socket.on(channels[2], (message) => {
            // Push out
            io.sockets.emit(channels[2], message);
        });
    }

});

// Run server
server.listen(3000);
console.log('Socket.IO Server 1.3 is running');
console.log('Echoing ' + channels.join(', '));