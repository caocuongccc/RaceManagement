using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class GoogleSheetsConfigDto
    {
        public string ServiceAccountKeyPath { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string DefaultSheetRange { get; set; } = "A:Z";
    }
}
