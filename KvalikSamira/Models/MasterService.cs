namespace KvalikSamira.Models
{
    public class MasterService
    {
        public int id { get; set; }
        public int userId { get; set; }
        public int serviceId { get; set; }
        public User user { get; set; } = null!;
        public Service service { get; set; } = null!;
    }
}
