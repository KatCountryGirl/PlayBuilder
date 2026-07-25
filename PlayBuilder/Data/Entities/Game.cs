using System.ComponentModel.DataAnnotations;

namespace PlayBuilder.Data.Entities;

public class Game
{
    [Key]
    public int Id { get; set; }

    [MaxLength(250)]
    public string Title { get; set; } = "";

    [MaxLength(250)]
    public string SortTitle { get; set; } = "";

    [MaxLength(100)]
    public string System { get; set; } = "";

    [MaxLength(50)]
    public string Region { get; set; } = "";

    [MaxLength(50)]
    public string Language { get; set; } = "";

    [MaxLength(25)]
    public string Revision { get; set; } = "";

    [MaxLength(25)]
    public string Version { get; set; } = "";

    public int DiscNumber { get; set; }

    [MaxLength(25)]
    public string Extension { get; set; } = "";

    public long FileSize { get; set; }

    [MaxLength(500)]
    public string SourcePath { get; set; } = "";

    [MaxLength(500)]
    public string RelativePath { get; set; } = "";

    [MaxLength(100)]
    public string Hash { get; set; } = "";

    public bool IsFavorite { get; set; }

    public DateTime Added { get; set; } = DateTime.UtcNow;

    public DateTime Modified { get; set; } = DateTime.UtcNow;
}