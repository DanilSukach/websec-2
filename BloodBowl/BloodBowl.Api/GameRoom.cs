using System.Collections.Concurrent;
using BloodBowl.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using BloodBowl.Api.Services;

namespace BloodBowl.Api;

public class GameRoom
{
    private static readonly ConcurrentDictionary<string, Player> Players = new();
    private static Star Star = new();
    private static readonly double GameLoopInterval = 1000 / 45;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly System.Timers.Timer _gameLoopTimer;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GameRoom(IHubContext<GameHub> hubContext, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _hubContext = hubContext;
        _gameLoopTimer = new System.Timers.Timer(GameLoopInterval);
        _gameLoopTimer.Elapsed += async (sender, e) => await UpdateGame();
        _gameLoopTimer.Start();
    }

    public async Task CheckGame()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var playerScoreService = scope.ServiceProvider.GetRequiredService<IPlayerScoreService>();
        var topPlayers = await playerScoreService.GetTopPlayers();
        await _hubContext.Clients.All.SendAsync("StarCollected", Star);
        await _hubContext.Clients.All.SendAsync("TopPlayers", topPlayers);
    }

    public async Task RegisterPlayer(string connectionId, string name)
    {
        if (Players.Count >= 10)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("Info", "full");
            return;
        }
        else
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("Info", "empty");
        }

        var player = new Player
        {
            Id = connectionId,
            Name = name,
            X = new Random().Next(100, 700),
            Y = new Random().Next(100, 500),
            Color = $"#{new Random().Next(0x1000000):X6}",
            VX = 0,
            VY = 0
        };

        Players[connectionId] = player;

        using var scope = _serviceScopeFactory.CreateScope();
        var playerScoreService = scope.ServiceProvider.GetRequiredService<IPlayerScoreService>();


        await playerScoreService.AddPlayerScoreAsync(connectionId, name);
    }

    public void MovePlayer(string connectionId, List<string> directions)
    {
        if (!Players.TryGetValue(connectionId, out var player)) return;

        float acceleration = 0.2f; 
        float maxSpeed = 3f;
        float oppositeAcceleration = 1.0f;

        foreach (var direction in directions)
        {
            switch (direction)
            {
                case "up":
                    player.VY -= player.VY > 0 ? oppositeAcceleration : acceleration;
                    break;
                case "down":
                    player.VY += player.VY < 0 ? oppositeAcceleration : acceleration;
                    break;
                case "left":
                    player.VX -= player.VX > 0 ? oppositeAcceleration : acceleration;
                    break;
                case "right":
                    player.VX += player.VX < 0 ? oppositeAcceleration : acceleration;
                    break;
            }
        }

        player.VX = Math.Clamp(player.VX, -maxSpeed, maxSpeed);
        player.VY = Math.Clamp(player.VY, -maxSpeed, maxSpeed);
    }

    public async Task UpdateGame()
    {
        float deceleration = 0.01f;
        float maxSpeed = 3f;

        foreach (var player in Players.Values)
        {
            if (player.VX > 0) player.VX -= deceleration;
            else if (player.VX < 0) player.VX += deceleration;

            if (player.VY > 0) player.VY -= deceleration;
            else if (player.VY < 0) player.VY += deceleration;
            if (Math.Abs(player.VX) < deceleration) player.VX = 0;
            if (Math.Abs(player.VY) < deceleration) player.VY = 0;

            player.VX = Math.Clamp(player.VX, -maxSpeed, maxSpeed);
            player.VY = Math.Clamp(player.VY, -maxSpeed, maxSpeed);

            player.X += player.VX;
            player.Y += player.VY;

            player.X = Math.Clamp(player.X, 0, 610);
            player.Y = Math.Clamp(player.Y, 0, 510);

            if (player.X < Star.X + Star.Radius && player.X + player.Width > Star.X - Star.Radius &&
                player.Y < Star.Y + Star.Radius && player.Y + player.Height > Star.Y - Star.Radius)
            {
                Star = new Star();
                await _hubContext.Clients.All.SendAsync("StarCollected", Star);
                using var scope = _serviceScopeFactory.CreateScope();
                var playerScoreService = scope.ServiceProvider.GetRequiredService<IPlayerScoreService>();

                await playerScoreService.IncrementStarCountAsync(player.Id);
                var topPlayers = await playerScoreService.GetTopPlayers();
                await _hubContext.Clients.All.SendAsync("TopPlayers", topPlayers);
            }

            foreach (var otherPlayer in Players.Values)
            {
                if (otherPlayer.Id != player.Id)
                {
                    if (player.X < otherPlayer.X + otherPlayer.Width && player.X + player.Width > otherPlayer.X &&
                        player.Y < otherPlayer.Y + otherPlayer.Height && player.Y + player.Height > otherPlayer.Y)
                    {
                        float tempVX = player.VX;
                        float tempVY = player.VY;

                        player.VX = otherPlayer.VX;
                        player.VY = otherPlayer.VY;

                        otherPlayer.VX = tempVX;
                        otherPlayer.VY = tempVY;

                        player.X += player.VX;
                        player.Y += player.VY;

                        otherPlayer.X += otherPlayer.VX;
                        otherPlayer.Y += otherPlayer.VY;


                        float overlapX = (player.Width + otherPlayer.Width) / 2 - Math.Abs(player.X - otherPlayer.X);
                        float overlapY = (player.Height + otherPlayer.Height) / 2 - Math.Abs(player.Y - otherPlayer.Y);

                        if (overlapX > 0 && overlapY > 0)
                        {
                            if (overlapX < overlapY)
                            {
                                player.X += overlapX / 2 * Math.Sign(player.X - otherPlayer.X);
                                otherPlayer.X -= overlapX / 2 * Math.Sign(player.X - otherPlayer.X);
                            }
                            else
                            {
                                player.Y += overlapY / 2 * Math.Sign(player.Y - otherPlayer.Y);
                                otherPlayer.Y -= overlapY / 2 * Math.Sign(player.Y - otherPlayer.Y);
                            }
                        }
                    }
                }
            }
        }
        await _hubContext.Clients.All.SendAsync("GameState", Players.Values);
    }

    public async Task RemovePlayer(string connectionId)
    {
        if (Players.TryRemove(connectionId, out _))
        {
            await _hubContext.Clients.All.SendAsync("PlayerLeft", connectionId);
        }
        if (Players.Count < 10) await _hubContext.Clients.All.SendAsync("Info", "empty");
    }
}
