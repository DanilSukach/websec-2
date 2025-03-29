using BloodBowl.Domain.Entities;

namespace BloodBowl.Api.Services;

public interface IPlayerScoreService
{
    Task AddPlayerScoreAsync(string connectionId, string name);
    Task IncrementStarCountAsync(string connectionId);
    Task<List<PlayerScore>> GetTopPlayers();
}
