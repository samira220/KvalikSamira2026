namespace KvalikSamira.Models
{
    public class Qualification
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string level { get; set; } = "Начинающий";
        public User user { get; set; } = null!;
    }
}
