using System;
using System.Collections.Generic;

namespace KvalikSamira.Data;

public partial class MasterService
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ServiceId { get; set; }

    public virtual Service Service { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
