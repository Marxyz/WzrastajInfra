using System;
using System.Collections.Generic;
using System.Text;
using infrastructure.Controllers.Requests;

namespace infrastructure.Models
{
    public class ArticleSavingProcessingModel
    {
        public IEnumerable<CategoryRequest> Categories { get; set; }
        public string AuthorGiveFilename { get; set; }
        public Guid AuthorUserId { get; set; }
        public string Content { get; set; }
        public IEnumerable<string> Links { get; set; }

        public Encoding Encoding { get; set; }
    }
}