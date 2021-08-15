using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Guid> Articles { get; set; }

        public static Category ProcessRequest(string id, string name)
        {
            var res = Guid.TryParse(id, out var idGuid);
            if (res)
            {
                return new Category { Id = idGuid, Name = name };
            }

            return new Category { Id = Guid.Empty, Name = name };
        }
    }
}