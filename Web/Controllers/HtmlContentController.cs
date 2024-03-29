﻿using System;
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

            return this.Content(mail.BodyHtml, "text/html");
        }
    }
}
