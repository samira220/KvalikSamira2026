using System.Collections.Generic;

namespace KvalikSamira.Models
{
    public class User
    {
        public int id { get; set; }
        public string lastName { get; set; } = string.Empty;
        public string firstName { get; set; } = string.Empty;
        public string? patronymic { get; set; }
        public string login { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public decimal balance { get; set; }
        public int roleId { get; set; }
        public Role role { get; set; } = null!;
        public Qualification? qualification { get; set; }
        public List<MasterService> masterServices { get; set; } = new();
        public List<Appointment> appointments { get; set; } = new();
        public List<Review> reviews { get; set; } = new();
        public List<QualificationRequest> qualificationRequests { get; set; } = new();
    }
}
