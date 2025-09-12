using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Interfaces
{
    public interface IEmailTemplateServices
    {
        Task<string> RenderTemplateAsync(string templateName, object model);
        Task<bool> TemplateExistsAsync(string templateName);
        Task<IEnumerable<string>> GetAvailableTemplatesAsync();
        string GetTemplateSubject(string templateName, object model);
    }
}
