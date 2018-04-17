using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/email")]
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendAsync([FromBody] EmailSendModel model)
        {
            bool result = await _emailService.SendAsync(model.InvoiceId, model.CheckoutUrl, model.Emails.Select(x => x.Trim()).ToList());

            return Ok(new DataResult
            {
                HasErrors = !result
            });
        }
    }
}
