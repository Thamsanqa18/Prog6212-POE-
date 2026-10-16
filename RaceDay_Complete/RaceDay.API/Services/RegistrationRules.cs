namespace RaceDay.API.Services;
public static class RegistrationRules
{
    public static bool IsAllowedRole(string? role) =>
        string.Equals(role, "Organiser", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role, "Participant", StringComparison.OrdinalIgnoreCase);

    public static string NormaliseRole(string role) =>
        string.Equals(role, "Organiser", StringComparison.OrdinalIgnoreCase) ? "Organiser" : "Participant";

    public static bool HasCapacity(int confirmedEnrolments, int maxParticipants) => confirmedEnrolments < maxParticipants;
}
