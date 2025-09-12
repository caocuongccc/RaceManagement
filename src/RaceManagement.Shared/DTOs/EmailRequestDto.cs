using RaceManagement.Abstractions.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class QueueEmailRequest
    {
        [Required]
        public int RegistrationId { get; set; }

        [Required]
        public EmailType EmailType { get; set; }

        public DateTime? ScheduledAt { get; set; }
    }

    public class BulkEmailRaceRequest
    {
        [Required]
        public EmailType EmailType { get; set; }

        public bool StopOnFirstError { get; set; } = false;
    }

    public class TestEmailRequest
    {
        [Required]
        [EmailAddress]
        public string To { get; set; } = string.Empty;

        public string? ToName { get; set; }
    }
}
