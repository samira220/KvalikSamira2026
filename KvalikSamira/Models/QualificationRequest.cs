using System;

namespace KvalikSamira.Models
{
    public class QualificationRequest
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string currentLevel { get; set; } = string.Empty;
        public string requestedLevel { get; set; } = string.Empty;
        public string status { get; set; } = "На рассмотрении";
        public DateTime createdAt { get; set; }
        public User user { get; set; } = null!;
    }
}
