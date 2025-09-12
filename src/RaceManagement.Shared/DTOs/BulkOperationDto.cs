using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class BulkCredentialOperation
    {
        public List<int> CredentialIds { get; set; } = new();
        public string Operation { get; set; } = string.Empty; // "activate", "deactivate", "delete", "test"
    }

    public class BulkSheetConfigOperation
    {
        public List<int> SheetConfigIds { get; set; } = new();
        public string Operation { get; set; } = string.Empty; // "activate", "deactivate", "delete", "test"
    }

    public class BulkOperationResult
    {
        public int TotalItems { get; set; }
        public int SuccessfulItems { get; set; }
        public int FailedItems { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<int, string> ItemResults { get; set; } = new(); // Id -> Status/Error message

        public bool IsFullSuccess => FailedItems == 0;
        public double SuccessRate => TotalItems > 0 ? (double)SuccessfulItems / TotalItems * 100 : 0;
    }
}
