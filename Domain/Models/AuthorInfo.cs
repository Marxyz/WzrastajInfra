using System;

namespace Domain.Models
{
    public class AuthorInfo
    {
        public string Mail { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}