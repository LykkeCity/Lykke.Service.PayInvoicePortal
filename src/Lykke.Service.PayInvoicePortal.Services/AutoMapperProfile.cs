using AutoMapper;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain.Payments;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<InvoiceModel, Payment>(MemberList.Destination)
                .ForMember(dest => dest.SettlementAsset,
                    opt => opt.Ignore());
        }
    }
}
