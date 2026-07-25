using Microsoft.EntityFrameworkCore;

namespace PlayBuilder.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(PlayBuilderDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        try
        {
            await db.Database.ExecuteSqlRawAsync(
                "ALTER TABLE Games ADD COLUMN IsFavorite INTEGER NOT NULL DEFAULT 0;");
        }
        catch
        {
            // The column already exists. SQLite does not support ADD COLUMN IF NOT EXISTS.
        }

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS Collections (
 Id INTEGER NOT NULL CONSTRAINT PK_Collections PRIMARY KEY AUTOINCREMENT,
 Name TEXT NOT NULL, Type TEXT NOT NULL, DestinationPath TEXT NOT NULL,
 Frontend TEXT NOT NULL, RuleJson TEXT NOT NULL, IsEnabled INTEGER NOT NULL,
 CreatedAt TEXT NOT NULL, UpdatedAt TEXT NOT NULL);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Collections_Name ON Collections(Name);
CREATE TABLE IF NOT EXISTS CollectionGames (
 CollectionId INTEGER NOT NULL, GameId INTEGER NOT NULL, AddedAt TEXT NOT NULL,
 CONSTRAINT PK_CollectionGames PRIMARY KEY (CollectionId, GameId),
 CONSTRAINT FK_CollectionGames_Collections_CollectionId FOREIGN KEY (CollectionId) REFERENCES Collections(Id) ON DELETE CASCADE,
 CONSTRAINT FK_CollectionGames_Games_GameId FOREIGN KEY (GameId) REFERENCES Games(Id) ON DELETE CASCADE);
CREATE INDEX IF NOT EXISTS IX_CollectionGames_GameId ON CollectionGames(GameId);
""");
    }
}
