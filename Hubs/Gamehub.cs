using Microsoft.AspNetCore.SignalR;

namespace MyFirstWebApp.Hubs;

// Handles real-time communication between clients and the server
public class GameHub : Hub
{
    // Method called by the host to create a room
    public async Task CreateRoom(string roomCode, int totalRounds)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        // Logic to store the room in a central GameService goes here...
        
        await Clients.Caller.SendAsync("RoomCreated", roomCode);
    }

    // Method called by other players to join
    public async Task JoinRoom(string roomCode, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        // Logic to add player to the room goes here...
        
        // Notify everyone in the room that a new player joined
        await Clients.Group(roomCode).SendAsync("PlayerJoined", playerName);
    }
}