using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Entities
{
    public class RaceShirtType : BaseEntity
    {
        public int RaceId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ShirtCategory { get; set; } = string.Empty;    // Nam, Nữ, Trẻ em

        [Required]
        [MaxLength(50)]
        public string ShirtType { get; set; } = string.Empty;        // T-Shirt, Singlet

        [Required]
        [MaxLength(255)]
        public string AvailableSizes { get; set; } = string.Empty;   // S,M,L,XL or KID-10,KID-12

        public decimal? Price { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Race Race { get; set; } = null!;

        // Helper methods
        public List<string> GetSizesList()
        {
            return AvailableSizes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => s.Trim())
                                 .ToList();
        }

        public bool IsValidSize(string size)
        {
            return GetSizesList().Contains(size, StringComparer.OrdinalIgnoreCase);
        }

        public string GetDisplayName()
        {
            return $"{ShirtCategory} {ShirtType}";
        }
    }
}
