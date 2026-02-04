using Shared.Enums;
using Shared.Models;

namespace Lecture.API.Models.Entities;

public class Attendance : BaseEntity
{
    public Guid LectureId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }

    public Lecture Lecture { get; set; } = null!;

}