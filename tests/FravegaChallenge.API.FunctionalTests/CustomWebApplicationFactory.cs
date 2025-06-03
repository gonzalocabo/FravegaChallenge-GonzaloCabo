using Ardalis.Result;
using FravegaChallenge.Domain.Entities;
using FravegaChallenge.Domain.Enums;
using FravegaChallenge.Infrastructure.Repositories.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq.Expressions;

namespace FravegaChallenge.API.FunctionalTests;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{

    private Mock<IOrdersRepository> GetOrdersRepositoryMock()
    {
        var ordersRepositoryMock = new Mock<IOrdersRepository>();

        ordersRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        ordersRepositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var buyerResult = Buyer.Create(
            firstName: "Juan",
            lastName: "Pérez",
            documentNumber: "12345678",
            phone: "+541112345678");

        var productResult = Product.Create(
            sku: "P001",
            name: "Producto A",
            description: "Descripción",
            price: 50m,
            quantity: 2);

        var orderResult = Order.Create(
            id: 1,
            externalReferenceId: "EXT123",
            originChannel: OriginChannel.Ecommerce,
            purchaseDate: DateTime.UtcNow,
            totalValue: productResult.Value.Price * productResult.Value.Quantity,
            buyer: buyerResult.Value,
            products: [productResult.Value]);

        ordersRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error(Infrastructure.InfrastructureErrors.OrdersRepository.GetAsyncGenericError));

        ordersRepositoryMock
            .Setup(x => x.GetAsync(It.Is<int>(x => x == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(orderResult));

        ordersRepositoryMock
            .Setup(x => x.GetByFiltersAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<Order> { orderResult.Value }));

        return ordersRepositoryMock;
    }

    private Mock<IEventsRepository> GetEventsRepositoryMock()
    {
        var eventsRepositoryMock = new Mock<IEventsRepository>();

        eventsRepositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        eventsRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<Event> { Event.Create(1, "event-001", EventType.PaymentReceived, DateTime.UtcNow, "adminUser123").Value }));

        eventsRepositoryMock
            .Setup(x => x.GetLastByOrderIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Event.Create(1, "event-001", EventType.PaymentReceived, DateTime.UtcNow, "adminUser123").Value));

        return eventsRepositoryMock;
    }

    private Mock<ICountersRepository> GetCountersRepositoryMock()
    {
        var countersRepositoryMock = new Mock<ICountersRepository>();

        countersRepositoryMock
            .Setup(x => x.GetNextSequenceValue(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(43));

        return countersRepositoryMock;
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            var ordersRepositoryMock = GetOrdersRepositoryMock();

            services.RemoveAll<IOrdersRepository>()
                .AddScoped(sp => ordersRepositoryMock.Object);

            var eventsRepositoryMock = GetEventsRepositoryMock();
            services.RemoveAll<IEventsRepository>()
                .AddScoped(sp => eventsRepositoryMock.Object);

            var countersRepositoryMock = GetCountersRepositoryMock();
            services.RemoveAll<ICountersRepository>()
                .AddScoped(sp => countersRepositoryMock.Object);
        });
    }
}
