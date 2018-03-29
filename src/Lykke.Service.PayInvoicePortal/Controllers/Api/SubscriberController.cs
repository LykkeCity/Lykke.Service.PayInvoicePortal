using System.Threading.Tasks;
using Lykke.Service.PayInvoicePortal.Core.Repositories;
using Lykke.Service.PayInvoicePortal.Models.Subscriber;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Route("/api/subscriber")]
    public class SubscriberController : Controller
    {
        private readonly ISubscriberRepository _subscriberRepository;

        public SubscriberController(ISubscriberRepository subscriberRepository)
        {
            _subscriberRepository = subscriberRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe([FromBody] SubscriberModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new SubscriberResultModel("Wrong email format"));
            }

            var subscriber = await _subscriberRepository.GetAsync(model.Email);

            if (subscriber == null)
            {
                await _subscriberRepository.CreateAsync(model);
                return Json(new SubscriberResultModel());
            }

            return Json(new SubscriberResultModel("You are already subscribed"));
        }
    }
}
