using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class Author
    {
        public AuthorInfo AuthorInfo { get; set; }
        public IEnumerable<Guid> Articles { get; set; }
        public Guid Id { get; set; }
    }
}