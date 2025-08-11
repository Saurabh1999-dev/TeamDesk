// Enum/LeaveEnums.cs
namespace TeamDesk.Enum
{
    public enum LeaveType
    {
        Annual = 1,
        Sick = 2,
        Personal = 3,
        Maternity = 4,
        Paternity = 5,
        Emergency = 6,
        Bereavement = 7,
        Study = 8
    }

    public enum LeaveStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Cancelled = 4
    }

    public static class LeaveExtensions
    {
        public static string GetLeaveTypeString(LeaveType leaveType)
        {
            return leaveType switch
            {
                LeaveType.Annual => "Annual Leave",
                LeaveType.Sick => "Sick Leave",
                LeaveType.Personal => "Personal Leave",
                LeaveType.Maternity => "Maternity Leave",
                LeaveType.Paternity => "Paternity Leave",
                LeaveType.Emergency => "Emergency Leave",
                LeaveType.Bereavement => "Bereavement Leave",
                LeaveType.Study => "Study Leave",
                _ => "Unknown"
            };
        }

        public static string GetLeaveStatusString(LeaveStatus status)
        {
            return status switch
            {
                LeaveStatus.Pending => "Pending",
                LeaveStatus.Approved => "Approved",
                LeaveStatus.Rejected => "Rejected",
                LeaveStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }

        public static string GetLeaveStatusColor(LeaveStatus status)
        {
            return status switch
            {
                LeaveStatus.Pending => "bg-yellow-100 text-yellow-800",
                LeaveStatus.Approved => "bg-green-100 text-green-800",
                LeaveStatus.Rejected => "bg-red-100 text-red-800",
                LeaveStatus.Cancelled => "bg-gray-100 text-gray-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }
    }
}
