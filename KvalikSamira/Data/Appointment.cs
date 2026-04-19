using System;
using System.Collections.Generic;

namespace KvalikSamira.Data;

public partial class Appointment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ServiceId { get; set; }

    public int MasterId { get; set; }

    public int QueueNumber { get; set; }

    public DateTime AppointmentDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User Master { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
