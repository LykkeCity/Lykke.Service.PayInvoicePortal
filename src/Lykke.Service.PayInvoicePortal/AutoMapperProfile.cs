using AutoMapper;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Domain.Payments;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.Assets;
using Lykke.Service.PayInvoicePortal.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Lykke.Service.PayInvoicePortal.Models.Payments;
using PaymentsResponse = Lykke.Service.PayInvoicePortal.Models.Payments.PaymentsResponse;

namespace Lykke.Service.PayInvoicePortal
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Asset, AssetModel>(MemberList.Destination);

            CreateMap<Payment, PaymentModel>(MemberList.Destination);

            CreateMap<Core.Domain.Payments.PaymentsResponse, PaymentsResponse>(MemberList.Destination);

            CreateMap<Invoice, ListItemModel>(MemberList.Destination)
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.SettlementAsset,
                    opt => opt.MapFrom(src => src.SettlementAsset.DisplayId))
                .ForMember(dest => dest.SettlementAssetAccuracy,
                    opt => opt.MapFrom(src => src.SettlementAsset.Accuracy));

            CreateMap<Invoice, InvoiceModel>(MemberList.Destination)
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.SettlementAsset,
                    opt => opt.MapFrom(src => src.SettlementAssetId))
                .ForMember(dest => dest.SettlementAssetDisplay,
                    opt => opt.MapFrom(src => src.SettlementAsset.DisplayId))
                .ForMember(dest => dest.SettlementAssetAccuracy,
                    opt => opt.MapFrom(src => src.SettlementAsset.Accuracy))
                .ForMember(dest => dest.PaymentAsset, opt => opt.MapFrom(src => src.PaymentAssetId))
                .ForMember(dest => dest.PaymentAssetNetwork, opt => opt.Ignore())
                .ForMember(dest => dest.Files,
                    opt => opt.Ignore())
                .ForMember(dest => dest.History,
                    opt=>opt.Ignore());

            CreateMap<HistoryItem, HistoryItemModel>(MemberList.Destination)
                .ForMember(dest => dest.Author,
                    opt => opt.MapFrom(src =>
                        src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : null))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.SettlementAsset,
                    opt => opt.MapFrom(src => src.SettlementAsset.DisplayId))
                .ForMember(dest => dest.SettlementAssetAccuracy,
                    opt => opt.MapFrom(src => src.SettlementAsset.Accuracy))
                .ForMember(dest => dest.PaymentAsset,
                    opt => opt.MapFrom(src => src.PaymentAsset.DisplayId))
                .ForMember(dest => dest.PaymentAssetAccuracy,
                    opt => opt.MapFrom(src => src.PaymentAsset.Accuracy));

            CreateMap<PaymentDetails, PaymentDetailsModel>(MemberList.Destination)
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.MerchantId,
                    opt => opt.MapFrom(src => src.Merchant.Id))
                .ForMember(dest => dest.Merchant,
                    opt => opt.MapFrom(src => src.Merchant.DisplayName))
                .ForMember(dest => dest.SettlementAsset,
                    opt => opt.MapFrom(src => src.SettlementAssetId))
                .ForMember(dest => dest.SettlementAssetDisplay,
                    opt => opt.MapFrom(src => src.SettlementAsset.DisplayId))
                .ForMember(dest => dest.SettlementAssetAccuracy,
                    opt => opt.MapFrom(src => src.SettlementAsset.Accuracy))
                .ForMember(dest => dest.PaymentAsset,
                    opt => opt.MapFrom(src => src.PaymentAssetId))
                .ForMember(dest => dest.PaymentAssetDisplay,
                    opt => opt.MapFrom(src => src.PaymentAsset.DisplayId))
                .ForMember(dest => dest.PaymentAssetAccuracy,
                    opt => opt.MapFrom(src => src.PaymentAsset.Accuracy))
                .ForMember(dest => dest.Files,
                    opt => opt.Ignore());

            CreateMap<PayInvoice.Client.Models.File.FileInfoModel, FileModel>(MemberList.Destination);
        }
    }
}
