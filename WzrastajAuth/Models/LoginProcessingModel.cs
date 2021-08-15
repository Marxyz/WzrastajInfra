namespace WzrastajAuth.Models
{
    public class LoginProcessingModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string RemoteIpAddress { get; set; }
    }
}