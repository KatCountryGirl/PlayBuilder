namespace PlayBuilder.Models;

public sealed record LibraryGameFilters(
    string? System,
    string Region,
    string Language,
    string Extension,
    string SearchText);
