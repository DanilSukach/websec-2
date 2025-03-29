using BloodBowl.Domain.Context;
using BloodBowl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace BloodBowl.Api.Services;

public class PlayerScoreService(BloodBowlDbContext context) : IPlayerScoreService
{
    public async Task AddPlayerScoreAsync(string connectionId, string name)
    {
        var newPlayer = new PlayerScore
        {
            ConnectionId = connectionId,
            Name = name,
            StarCount = 0
        };
        context.PlayerScore.Add(newPlayer);
        await context.SaveChangesAsync();
    }

    public async Task IncrementStarCountAsync(string connectionId)
    {
        var playerScore = context.PlayerScore.FirstOrDefault(p => p.ConnectionId == connectionId);
        if (playerScore != null)
        {
            playerScore.StarCount++;
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<PlayerScore>> GetTopPlayers()
    {
        var topPlayers = await context.PlayerScore
            .OrderByDescending(p => p.StarCount)
            .Take(10)
            .ToListAsync();

        return topPlayers;
    }
}
