using FravegaChallenge.Application.Internationalization;
using FravegaChallenge.Application.Responses;
using FravegaChallenge.Domain.Entities;
using Mapster;
using Microsoft.Extensions.Localization;

namespace Microsoft.Extensions.DependencyInjection;

internal static class MappingConfig
{
    internal static void AddCustomMappings(this TypeAdapterConfig typeAdapterConfig)
    {
        typeAdapterConfig.NewConfig<Order, GetOrderResponse>()
            .Map(x => x.Channel, x => x.OriginChannel)
            .AfterMapping(dto => 
            {
                var localizer = MapContext.Current.GetService<IStringLocalizer<ApplicationTranslations>>();
                dto.ChannelTranslate = localizer[dto.Channel.ToString()].Value;
                dto.StatusTranslate = localizer[dto.Status.ToString()].Value;
            });

        typeAdapterConfig.NewConfig<Buyer, GetOrderBuyer>();
        typeAdapterConfig.NewConfig<Product, GetOrderProduct>();
        typeAdapterConfig.NewConfig<Event, GetOrderEvent>()
            .Map(x => x.Id, x => x.EventId);

    }
}
