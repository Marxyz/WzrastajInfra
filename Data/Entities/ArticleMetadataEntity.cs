using System.Collections.Generic;

namespace Data.Entities
{
    public class ArticleMetadataEntity : BaseEntity
    {
        public UserEntity Author { get; set; }
        public string Title { get; set; }
        public ICollection<CategoryEntity> Categories { get; set; }
    }
}