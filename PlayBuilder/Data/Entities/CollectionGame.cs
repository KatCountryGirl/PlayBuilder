namespace PlayBuilder.Data.Entities;

public class CollectionGame
{
    public int CollectionId { get; set; }
    public Collection Collection { get; set; } = null!;
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
