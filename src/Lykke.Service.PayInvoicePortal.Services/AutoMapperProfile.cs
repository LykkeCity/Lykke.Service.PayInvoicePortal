using AutoMapper;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain.Payments;
using Lykke.Service.PayInvoicePortal.Services.Extensions;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<InvoiceModel, Payment>(MemberList.Destination)
                .ForMember(dest => dest.SettlementAsset, opt => opt.Ignore());

            CreateMap<PaymentRequestModel, Payment>(MemberList.Destination)
                .ForMember(dest => dest.Number, opt => opt.Ignore())
                .ForMember(dest => dest.ClientName, opt => opt.Ignore())
                .ForMember(dest => dest.ClientEmail, opt => opt.Ignore())
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToInvoiceStatus(src.ProcessingError)))
                .ForMember(dest => dest.SettlementAsset, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.Timestamp));
        }
    }
}
