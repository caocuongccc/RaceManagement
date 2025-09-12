using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class SheetMetadataDto
    {
        public string SpreadsheetId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int SheetCount { get; set; }
        public List<SheetInfoDto> Sheets { get; set; } = new();
        public DateTime LastModified { get; set; }
        public string Owner { get; set; } = string.Empty;
        public List<string> SheetNames => Sheets.Select(s => s.Title).ToList();

        // Computed properties
        public int TotalRows => Sheets.Sum(s => s.RowCount);
        public string SheetsDisplay => string.Join(", ", Sheets.Select(s => s.Title));
    }
    public class SheetInfoDto
{
    public int SheetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public string SheetType { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
}
}
