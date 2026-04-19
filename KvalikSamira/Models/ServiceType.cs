using System.Collections.Generic;

namespace KvalikSamira.Models
{
    public class ServiceType
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public List<Service> services { get; set; } = new();
    }
}
