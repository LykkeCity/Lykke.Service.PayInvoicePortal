using System.Threading.Tasks;
using Lykke.Service.PayInvoicePortal.Core.Domain;
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

            Subscriber subscriber = await _subscriberRepository.GetAsync(model.Email);

            if (subscriber != null)
            {
                return Json(new SubscriberResultModel("You are already subscribed"));
            }

            await _subscriberRepository.InsertAsync(new Subscriber
            {
                Email = model.Email
            });

            return Json(new SubscriberResultModel());
        }
    }
}
