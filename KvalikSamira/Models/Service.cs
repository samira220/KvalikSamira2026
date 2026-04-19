using System;
using System.Collections.Generic;

namespace KvalikSamira.Models
{
    public class Service
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string? description { get; set; }
        public decimal price { get; set; }
        public int durationMinutes { get; set; }
        public string? imagePath { get; set; }
        public int serviceTypeId { get; set; }
        public int? collectionId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public ServiceType serviceType { get; set; } = null!;
        public Collection? collection { get; set; }
        public List<MasterService> masterServices { get; set; } = new();
        public List<Appointment> appointments { get; set; } = new();
        public List<Review> reviews { get; set; } = new();
    }
}
