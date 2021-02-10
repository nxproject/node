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

    // Setup internal channel
    socket.on(channels[0], (message) => {
        // Tell world
        //console.log('Message : ', message);
        // Push out
        io.sockets.emit(channels[0], message);
    });
    console.log('Echoing ' + channels[0] + ' as internal channel');

    // Setup account channel
    if (channels.length > 1) {
        socket.on(channels[1], (message) => {
            // Tell world
            //console.log('Message : ', message);
            // Push out
            io.sockets.emit(channels[1], message);
        });
        console.log('Echoing ' + channels[1] + ' as accounts channel');
    }

});

// Run server
server.listen(3000);
console.log('Socket.IO server is running');