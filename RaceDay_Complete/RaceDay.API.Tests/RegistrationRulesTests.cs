using RaceDay.API.Services;
using Xunit;

namespace RaceDay.API.Tests;

public class RegistrationRulesTests
{
    [Theory]
    [InlineData("Organiser", true)]
    [InlineData("Participant", true)]
    [InlineData("organiser", true)]
    [InlineData("Admin", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsAllowedRole_OnlyAcceptsSupportedRoles(string? role, bool expected) =>
        Assert.Equal(expected, RegistrationRules.IsAllowedRole(role));

    [Theory]
    [InlineData(0, 10, true)]
    [InlineData(9, 10, true)]
    [InlineData(10, 10, false)]
    [InlineData(11, 10, false)]
    public void HasCapacity_RejectsFullEvents(int enrolments, int maximum, bool expected) =>
        Assert.Equal(expected, RegistrationRules.HasCapacity(enrolments, maximum));
}
