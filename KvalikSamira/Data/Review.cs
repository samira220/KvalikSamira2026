using System;
using System.Collections.Generic;

namespace KvalikSamira.Data;

public partial class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? ServiceId { get; set; }

    public int? MasterId { get; set; }

    public string Text { get; set; } = null!;

    public int Rating { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? Master { get; set; }

    public virtual Service? Service { get; set; }

    public virtual User User { get; set; } = null!;
}
