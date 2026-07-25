using System.ComponentModel.DataAnnotations;

namespace PlayBuilder.Data.Entities;

public class Collection
{
    [Key] public int Id { get; set; }
    [MaxLength(160)] public string Name { get; set; } = "";
    [MaxLength(40)] public string Type { get; set; } = "custom";
    [MaxLength(500)] public string DestinationPath { get; set; } = "";
    [MaxLength(100)] public string Frontend { get; set; } = "";
    public string RuleJson { get; set; } = "{}";
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<CollectionGame> Games { get; set; } = [];
}
