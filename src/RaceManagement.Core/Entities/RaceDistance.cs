using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Entities
{
    public class RaceDistance : BaseEntity
    {
        public int RaceId { get; set; }
        public string Distance { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? BibPrefix { get; set; }
        public int? MaxParticipants { get; set; }

        // Navigation properties
        public virtual Race Race { get; set; } = null!;
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}
