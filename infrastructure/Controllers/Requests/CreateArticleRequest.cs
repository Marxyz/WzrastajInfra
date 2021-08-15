using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace infrastructure.Controllers.Requests
{
    public class CreateArticleRequest
    {
        [Required] public string Name { get; set; }
        [Required] public string AuthorId { get; set; }
        public IEnumerable<string> Links { get; set; } = new List<string>();
        public IEnumerable<CategoryRequest> Categories { get; set; } = new List<CategoryRequest>();
        [Required] public string Content { get; set; }
    }
}