using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions.Models
{
    public abstract class Auditable
    {
        public long CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public long UpdatedById { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long DeletedById { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}