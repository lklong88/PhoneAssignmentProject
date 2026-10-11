using System.Text.Json;
using CUSoCalPhoneAssignments.Models;
namespace CUSoCalPhoneAssignments.Services;
public sealed class AssignmentStore
{
    private readonly object _gate = new();
    private readonly string _path;
    private readonly PhoneData _data;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public AssignmentStore(IWebHostEnvironment env, IConfiguration configuration)
    {
        var folder = configuration["DATA_DIRECTORY"];
        if (string.IsNullOrWhiteSpace(folder)) folder = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(folder);
        _path = Path.Combine(folder, "assignments.json");
        var seed = Path.Combine(env.ContentRootPath, "Data", "seed.json");
        _data = JsonSerializer.Deserialize<PhoneData>(File.ReadAllText(File.Exists(_path) ? _path : seed), JsonOptions) ?? throw new InvalidOperationException("Invalid data");
    }
    public DashboardViewModel GetDashboard()
    {
        lock (_gate)
        {
            var occupied = _data.Assignments.Select(a => a.PhoneNumberId).ToHashSet();
            return new DashboardViewModel
            {
                AssignedCount = _data.Assignments.Count,
                AvailableCount = _data.PhoneNumbers.Count - occupied.Count,
                PhoneNumbers = _data.PhoneNumbers.Select(p => new PhoneNumber { Id = p.Id, Number = p.Number }).ToList(),
                Members = _data.TeamMembers.Select(m =>
                {
                    var a = _data.Assignments.FirstOrDefault(a => a.TeamMemberId == m.Id);
                    return new MemberRow { Id = m.Id, Name = $"{m.FirstName} {m.LastName}", PhoneId = a?.PhoneNumberId,
                        PhoneNumber = _data.PhoneNumbers.FirstOrDefault(p => p.Id == a?.PhoneNumberId)?.Number,
                        Choices = _data.PhoneNumbers.Where(p => !occupied.Contains(p.Id) || p.Id == a?.PhoneNumberId).Select(p => new PhoneNumber { Id = p.Id, Number = p.Number }).ToList() };
                }).ToList()
            };
        }
    }
    public string? Assign(int memberId, int phoneId)
    {
        lock (_gate)
        {
            if (!_data.TeamMembers.Any(m => m.Id == memberId)) return "Employee not found.";
            if (!_data.PhoneNumbers.Any(p => p.Id == phoneId)) return "Please select a valid phone number.";
            if (_data.Assignments.Any(a => a.PhoneNumberId == phoneId && a.TeamMemberId != memberId)) return "This phone number is already assigned to another employee.";
            var previous = _data.Assignments.FirstOrDefault(a => a.TeamMemberId == memberId);
            if (previous?.PhoneNumberId == phoneId) return null;
            if (previous is not null) previous.PhoneNumberId = phoneId;
            else _data.Assignments.Add(new Assignment { TeamMemberId = memberId, PhoneNumberId = phoneId });
            Save(); return null;
        }
    }
    public string? Remove(int memberId)
    {
        lock (_gate)
        {
            if (!_data.TeamMembers.Any(m => m.Id == memberId)) return "Employee not found.";
            _data.Assignments.RemoveAll(a => a.TeamMemberId == memberId);
            Save(); return null;
        }
    }
    private void Save()
    {
        var temp = _path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(_data, JsonOptions));
        File.Move(temp, _path, true);
    }
}
