using RazorEngine;
using RazorEngine.Templating;
using SharePointPnP.ProvisioningApp.MailTemplates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure.Mail
{
    /// <summary>
    /// Helper static class to generate on the fly a dynamic HTML body for an email
    /// </summary>
    public static class MailBodyHandler
    {
        public static String GetMailMessage(string templateName, object model)
        {
            // Get the template
            String template = GetTemplateFromResource(templateName);

            // Cache the template and run the engine
            var html = Engine.Razor.RunCompile(template, templateName, null, model);

            return (html);
        }

        private static String GetTemplateFromResource(string name)
        {
            // If it's not cached get it from the Bus Assembly
            var assembly = typeof(MailTemplatesRoot).Assembly;
            var resourceName = $"{typeof(MailTemplatesRoot).Namespace}.Templates.{name}.cshtml";

            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(resourceStream))
            {
                return (reader.ReadToEnd());
            }
        }
    }
}
