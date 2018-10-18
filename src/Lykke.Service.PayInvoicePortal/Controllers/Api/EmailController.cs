using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInvoicePortal.Constants;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Models.Email;
using LykkePay.Common.Validation;
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
        [ValidateModel]
        public async Task<IActionResult> SendAsync([FromBody] EmailSendModel model)
        {
            foreach (var email in model.Emails)
            {
                if (!email.IsValidEmail())
                {
                    return BadRequest(ErrorResponse.Create("Some of the emails is not valid"));
                }
            }

            bool result = await _emailService.SendAsync(model.InvoiceId, model.CheckoutUrl, model.Emails.Select(x => x.Trim()).ToList());

            if (result)
            {
                return NoContent();
            }

            return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.UnexpectedError));
        }
    }
}
