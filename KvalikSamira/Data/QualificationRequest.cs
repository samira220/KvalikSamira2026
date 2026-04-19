using System;
using System.Collections.Generic;

namespace KvalikSamira.Data;

public partial class QualificationRequest
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string CurrentLevel { get; set; } = null!;

    public string RequestedLevel { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
