using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class UpdateLastSyncRowRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Row index must be greater than 0")]
        public int RowIndex { get; set; }
    }
}
