namespace infrastructure
{
    public class MailSenderConfig
    {
        public string ConfirmRegistrationBaseUrl { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ApiKey { get; set; }
        public string TemplateId { get; set; }
    }
}