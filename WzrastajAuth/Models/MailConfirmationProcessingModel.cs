using System;

namespace WzrastajAuth.Models
{
    public class MailConfirmationProcessingModel
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}