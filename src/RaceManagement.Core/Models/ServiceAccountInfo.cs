using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Models
{
    public class ServiceAccountInfo
    {
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string PrivateKeyId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class CredentialValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; set; } = new();
        public ServiceAccountInfo? ServiceAccountInfo { get; set; }

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public string GetErrorsString()
        {
            return string.Join("; ", Errors);
        }
    }
}
