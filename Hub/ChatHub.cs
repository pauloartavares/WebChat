using Microsoft.AspNetCore.SignalR;
using WebChat.Model;

namespace WebChat.Hub
{
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        // Storing user connection in my server
        private readonly IDictionary<string, UserConnection> _connection;

        // Injecting it in the ctor
        public ChatHub(IDictionary<string, UserConnection> connection)
        {
            _connection = connection;
        }


        // User joins the Room
        public async Task JoinRoom(UserConnection userConnection)
        {
            // "Groups" comes from the Hub class,
            // and it is added to access the properties of Hub
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);

            //Once the user is added to the group it will also in my connections
            _connection[Context.ConnectionId] = userConnection;

            // Notifying the client
            await Clients.Group(userConnection.Room!)
                .SendAsync("ReceiveMessage", $"{userConnection.UserName} has joined the chat.");
            await SendConnectedUser(userConnection.Room!);
        }

        // User sends a message to a room
        public async Task SendMessage(string message)
        {
            if(_connection.TryGetValue(Context.ConnectionId, out UserConnection userConnection)) 
            {
                //The client will send the message for this room
                await Clients.Group(userConnection.Room!)
                    .SendAsync("ReceiveMessage", userConnection.UserName, message, DateTime.Now);
            }
        }

        //Handling the disconnection, sending a message of disconnected user
        // OnDisconnectedAsync is a method comprehended inside the Hub class
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (!_connection.TryGetValue(Context.ConnectionId, out UserConnection roomConnection))
            {
                return base.OnDisconnectedAsync(exception);
            }
            Clients.Group(roomConnection.Room!)
                .SendAsync("ReceivedMessage", $"{roomConnection.UserName} has left the chat");
            SendConnectedUser(roomConnection.Room!);
            return base.OnDisconnectedAsync(exception);
        }

        // Notifying how many users are connected in the room
        // This methods gets the room name, wich I want all the list of users
        public Task SendConnectedUser(string Room)
        {
            var users = _connection.Values
                .Where(u => u.Room == Room)
                .Select(s => s.UserName);
            // Takes the list of users and returns it to the client           
            return Clients.Group(Room).SendAsync("ConnectedUser", users);
            // The client will look for the "ConnectedUsers" method and it will be invoked in the angular once it is created
        }

    }
}
