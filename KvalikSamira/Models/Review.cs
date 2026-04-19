using System;

namespace KvalikSamira.Models
{
    public class Review
    {
        public int id { get; set; }
        public int userId { get; set; }
        public int? serviceId { get; set; }
        public int? masterId { get; set; }
        public string text { get; set; } = string.Empty;
        public int rating { get; set; }
        public DateTime createdAt { get; set; }
        public User user { get; set; } = null!;
        public Service? service { get; set; }
        public User? master { get; set; }
    }
}
