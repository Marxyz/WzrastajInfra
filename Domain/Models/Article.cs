using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class Article
    {
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime Modification { get; set; }
        public ArticleContent Content { get; set; }
        public IEnumerable<Guid> Categories { get; set; }
    }
}