using System.Text.Json.Serialization;

namespace TeamDesk.Enum
{
    [JsonConverter(typeof(JsonNumberEnumConverter<ProjectStatus>))]
    public enum ProjectStatus
    {
        Planning = 0,
        Active = 1,
        OnHold = 2,
        Completed = 3,
        Cancelled = 4
    }
    [JsonConverter(typeof(JsonNumberEnumConverter<ProjectPriority>))]
    public enum ProjectPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
    [JsonConverter(typeof(JsonNumberEnumConverter<ProjectRole>))]

    public enum ProjectRole
    {
        TeamMember = 0,
        ProjectLead = 1,
        TechnicalLead = 2,
        ProjectManager = 3
    }
}
