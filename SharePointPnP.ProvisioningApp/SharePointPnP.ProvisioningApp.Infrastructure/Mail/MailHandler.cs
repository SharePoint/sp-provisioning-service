using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure.Mail
{
    /// <summary>
    /// Helper static class to send an email message via Microsoft Graph
    /// </summary>
    public static class MailHandler
    {
        public static void SendMailNotification(String bodyTemplateName, String destinationAddress, String[] additionalRecipients, object mailBody, string graphAccessToken)
        {
            if (additionalRecipients == null)
            {
                additionalRecipients = new String[0];
            }

            var mailSenderUPN = ConfigurationManager.AppSettings["OfficeDevPnP:MailSenderUPN"];
            var mailFrom = ConfigurationManager.AppSettings["OfficeDevPnP:MailFrom"];
            var mailSubject = ConfigurationManager.AppSettings["OfficeDevPnP:MailSubject"];

            if (!String.IsNullOrEmpty(mailSenderUPN))
            {
                HttpHelper.MakePostRequest($"https://graph.microsoft.com/v1.0/users/{mailSenderUPN}/sendMail",
                    new
                    {
                        message = new
                        {
                            subject = mailSubject,
                            body = new
                            {
                                contentType = "HTML",
                                content = MailBodyHandler.GetMailMessage(bodyTemplateName, mailBody)
                            },
                            from = new
                            {
                                emailAddress = new
                                {
                                    address = mailFrom
                                }
                            },
                            toRecipients = new []
                            {
                                new {
                                    emailAddress = new
                                    {
                                        address = destinationAddress
                                    }
                                }
                            },
                            bccRecipients = (from ar in additionalRecipients select new
                            {
                                emailAddress = new
                                {
                                    address = ar
                                }
                            }).ToArray(),
                        },
                        saveToSentItems = false
                    }, "application/json", graphAccessToken);
            }
        }
    }
}
