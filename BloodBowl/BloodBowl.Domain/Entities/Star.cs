namespace BloodBowl.Domain.Entities;

/// <summary>
/// Класс звезды
/// </summary>
public class Star
{
    private static readonly Random Random = new();
    /// <summary>
    /// Позиция по X
    /// </summary>
    public float X { get; set; } = Random.Next(50, 550);
    /// <summary>
    /// Позиция по Y
    /// </summary>
    public float Y { get; set; } = Random.Next(50, 450);
    /// <summary>
    /// Радиус
    /// </summary>
    public float Radius { get; set; } = 15f;
}
