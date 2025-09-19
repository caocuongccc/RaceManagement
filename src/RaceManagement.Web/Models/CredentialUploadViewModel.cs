using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceManagement.Web.Models
{
    public class CredentialUploadViewModel
    {
        [Required]
        public string Name { get; set; } = "";

        [Required]
        [Display(Name = "File JSON Credential")]
        public IFormFile? CredentialFile { get; set; }
    }
}
