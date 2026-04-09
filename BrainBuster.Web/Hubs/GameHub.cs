using Microsoft.AspNetCore.SignalR;

namespace BrainBuster.Web.Hubs;

public class GameHub : Hub
{
    // In this simplified hot-seat mode, the hub is only used for broadcasting
    // the state to the single client. No client methods are needed.
}
