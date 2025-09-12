using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class SelectListDto
    {
        public int Value { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public object? Extra { get; set; }
    }

    public class CredentialSelectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ServiceAccountEmail { get; set; } = string.Empty;
        public int SheetConfigCount { get; set; }
        public bool IsActive { get; set; }
        public bool FileExists { get; set; }

        public string DisplayText => $"{Name} ({ServiceAccountEmail})";
        public string StatusText => IsActive ? (FileExists ? "✅" : "❌") : "⏸️";
    }

    public class SheetConfigSelectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SpreadsheetId { get; set; } = string.Empty;
        public string CredentialName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? LastSyncRowIndex { get; set; }
        public int RaceCount { get; set; }

        public string DisplayText => $"{Name} ({CredentialName})";
        public string StatusText => IsActive ? "✅" : "⏸️";
        public string SyncInfo => LastSyncRowIndex.HasValue
            ? $"Sync: dòng {LastSyncRowIndex}"
            : "Chưa sync";
    }
}
