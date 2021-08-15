using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace infrastructure.Controllers.Requests
{
    public class CategoryRequest : IValidatableObject
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Id))
            {
                yield return new ValidationResult($"Either {nameof(Name)} or {nameof(Id)} has to be set.");
            }
        }
    }
}