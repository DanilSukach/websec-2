using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloodBowl.Domain.Entities;

/// <summary>
/// Класc прогресса игрока
/// </summary>
[Table("PlayerScores")]
public class PlayerScore
{
    /// <summary>
    /// Идентификатор соединения
    /// </summary>
    [Key]
    [Column("connection_id")]
    public required string ConnectionId { get; set; }
    /// <summary>
    /// Имя игрока
    /// </summary>
    [Column("name")]
    [Required]
    public required string Name { get; set; }
    /// <summary>
    /// Количество звезд
    /// </summary>
    [Column("star_count")]
    public int StarCount { get; set; } = 0;
}
