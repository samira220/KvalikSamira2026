using System;
using System.Collections.Generic;

namespace KvalikSamira.Data;

public partial class User
{
    public int Id { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public decimal Balance { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<Appointment> AppointmentMasters { get; set; } = new List<Appointment>();

    public virtual ICollection<Appointment> AppointmentUsers { get; set; } = new List<Appointment>();

    public virtual ICollection<MasterService> MasterServices { get; set; } = new List<MasterService>();

    public virtual Qualification? Qualification { get; set; }

    public virtual ICollection<QualificationRequest> QualificationRequests { get; set; } = new List<QualificationRequest>();

    public virtual ICollection<Review> ReviewMasters { get; set; } = new List<Review>();

    public virtual ICollection<Review> ReviewUsers { get; set; } = new List<Review>();

    public virtual Role Role { get; set; } = null!;
}
