namespace CUSoCalPhoneAssignments.Models;
public sealed class PhoneData
{
    public List<TeamMember> TeamMembers { get; set; } = new();
    public List<PhoneNumber> PhoneNumbers { get; set; } = new();
    public List<Assignment> Assignments { get; set; } = new();
}
public sealed class TeamMember { public int Id { get; set; } public string FirstName { get; set; } = ""; public string LastName { get; set; } = ""; }
public sealed class PhoneNumber { public int Id { get; set; } public string Number { get; set; } = ""; }
public sealed class Assignment { public int TeamMemberId { get; set; } public int PhoneNumberId { get; set; } }
public sealed class DashboardViewModel { public List<MemberRow> Members { get; set; } = new(); public List<PhoneNumber> PhoneNumbers { get; set; } = new(); public int AssignedCount { get; set; } public int AvailableCount { get; set; } }
public sealed class MemberRow { public int Id { get; set; } public string Name { get; set; } = ""; public int? PhoneId { get; set; } public string? PhoneNumber { get; set; } public List<PhoneNumber> Choices { get; set; } = new(); }
