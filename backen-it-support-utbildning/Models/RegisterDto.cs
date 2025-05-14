namespace backen_it_support_utbildning.Models
{
    public class RegisterDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Category { get; set; } = string.Empty;
    }
}
