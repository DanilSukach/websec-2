namespace BloodBowl.Domain.Entities;

/// <summary>
/// Класс игрока
/// </summary>
public class Player
{
    /// <summary>
    /// Идентификатор игрока
    /// </summary>
    public required string Id { get; set; }
    /// <summary>
    /// Имя игрока
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Стартовая позиция по X
    /// </summary>
    public required float X { get; set; }
    /// <summary>
    /// Стартовая позиция по Y
    /// </summary>
    public required float Y { get; set; }
    /// <summary>
    /// Текущая позиция по X
    /// </summary>
    public float VX { get; set; }
    /// <summary>
    /// Текущая позиция по Y
    /// </summary>
    public float VY { get; set; }
    /// <summary>
    /// Цвет игрока
    /// </summary>
    public required string Color { get; set; }
    /// <summary>
    /// Ширина машинки
    /// </summary>
    public float Width { get; set; } = 35f;
    /// <summary>
    /// Высота машинки
    /// </summary>
    public float Height { get; set; } = 35f;
}
