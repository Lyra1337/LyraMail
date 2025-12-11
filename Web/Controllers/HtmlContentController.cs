using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Lyralabs.TempMailServer.Web.Controllers
{
    public class HtmlContentController : Controller
    {
        private readonly MailboxService mailboxService;

        public HtmlContentController(MailboxService mailboxService)
        {
            this.mailboxService = mailboxService;
        }

        [HttpGet, Microsoft.AspNetCore.Mvc.Route("/Api/HtmlContent/{account}/{secret}")]
        public async Task<IActionResult> HtmlContent(
            [FromRoute] string account,
            [FromRoute] Guid secret,
            [FromQuery] string privateKey)
        {
            var mail = await this.mailboxService.GetDecryptedMail(account, secret, privateKey);

            var htmlContent = WrapWithStyles(mail.BodyHtml);

            return this.Content(htmlContent, "text/html");
        }

        private static string WrapWithStyles(string htmlContent)
        {
            const string styleWrapper = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {
            padding: 1rem;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            max-width: 100%;
            overflow-wrap: break-word;
            word-wrap: break-word;
        }
        * {
            max-width: 100%;
        }
        img {
            height: auto;
        }
        table {
            width: auto !important;
            max-width: 100%;
        }
    </style>
</head>
<body>
{0}
</body>
</html>";

            if (htmlContent?.Contains("<html", StringComparison.OrdinalIgnoreCase) == true ||
                htmlContent?.Contains("<body", StringComparison.OrdinalIgnoreCase) == true)
            {
                return htmlContent;
            }

            return string.Format(styleWrapper, htmlContent);
        }
    }
}
