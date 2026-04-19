using System;
using System.Collections.Generic;

namespace KvalikSamira.Data;

public partial class Qualification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Level { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
