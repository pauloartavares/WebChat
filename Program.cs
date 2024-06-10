using WebChat.Hub;
using WebChat.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Addind the SignalR to connect the clients in real time
builder.Services.AddSignalR();
//Adding the dependency injection for my IDictionary
//This will always create a new instance
builder.Services.AddSingleton<IDictionary<string, UserConnection>>(opt => 
    new Dictionary<string, UserConnection>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Adding Endpoint 
app.UseEndpoints(endpoint =>
{
    endpoint.MapHub<ChatHub>("/chat");
});


app.MapControllers();

app.Run();
