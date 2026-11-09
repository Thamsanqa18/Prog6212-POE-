namespace RaceDay.Api.Models;

/// <summary>The kind of road event.</summary>
public enum EventType
{
    Run,
    Walk,
    Cycle
}

/// <summary>Whether a category groups participants by age or by distance.</summary>
public enum CategoryType
{
    Age,
    Distance
}

/// <summary>The two user roles supported by the system.</summary>
public static class AppRoles
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";

    public static bool TryNormalise(string? value, out string role)
    {
        role = string.Empty;
        if (string.Equals(value?.Trim(), Organiser, StringComparison.OrdinalIgnoreCase))
        {
            role = Organiser;
            return true;
        }
        if (string.Equals(value?.Trim(), Participant, StringComparison.OrdinalIgnoreCase))
        {
            role = Participant;
            return true;
        }
        return false;
    }
}
