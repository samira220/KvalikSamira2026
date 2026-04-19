using System.Collections.Generic;

namespace KvalikSamira.Models
{
    public class Role
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public List<User> users { get; set; } = new();
    }
}
