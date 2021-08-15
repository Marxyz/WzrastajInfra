using System.Collections.Generic;

namespace Data.Entities
{
    public class CategoryEntity : BaseEntity
    {
        public string Name { get; set; }
        public ICollection<ArticleMetadataEntity> Articles { get; set; }
    }
}