using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class CredentialExportDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ServiceAccountEmail { get; set; } = string.Empty;
        // Note: Never export actual credential file path for security
    }

    public class SheetConfigExportDto
    {
        public string Name { get; set; } = string.Empty;
        public string SpreadsheetId { get; set; } = string.Empty;
        public string SheetName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CredentialName { get; set; } = string.Empty; // Reference by name
        public int HeaderRowIndex { get; set; }
        public int DataStartRowIndex { get; set; }
    }

    public class CredentialImportDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile CredentialFile { get; set; } = null!;
        public List<SheetConfigImportDto> SheetConfigs { get; set; } = new();
    }

    public class SheetConfigImportDto
    {
        public string Name { get; set; } = string.Empty;
        public string SpreadsheetId { get; set; } = string.Empty;
        public string SheetName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int HeaderRowIndex { get; set; } = 1;
        public int DataStartRowIndex { get; set; } = 2;
    }
}
