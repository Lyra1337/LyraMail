using System;
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

        [HttpGet, Route("/Api/HtmlContent/{account}/{secret}")]
        public IActionResult HtmlContent([FromRoute] string account, [FromRoute] Guid secret, [FromQuery] string privateKey)
        {
            var mail = this.mailboxService.GetMail(account, secret, privateKey);

            return this.Content(mail.BodyHtml, "text/html");
        }
    }
}
