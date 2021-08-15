using System;

namespace infrastructure.Models
{
    public class MailConfirmationProcessingModel
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}