using System;

namespace KvalikSamira.Models
{
    public class Appointment
    {
        public int id { get; set; }
        public int userId { get; set; }
        public int serviceId { get; set; }
        public int masterId { get; set; }
        public int queueNumber { get; set; }
        public DateTime appointmentDate { get; set; }
        public DateTime createdAt { get; set; }
        public User user { get; set; } = null!;
        public Service service { get; set; } = null!;
        public User master { get; set; } = null!;
    }
}
