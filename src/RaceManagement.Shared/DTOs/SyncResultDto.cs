using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class SyncResultDto
    {
        public int RaceId { get; set; }
        public bool Success { get; set; }
        public int Added { get; set; }
        public int Skipped { get; set; }
        public string? Error { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime SyncTime { get; set; } = DateTime.Now;
    }
}
