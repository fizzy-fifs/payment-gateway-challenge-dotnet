using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTestFixture
{
    public WebApplicationFactory<PaymentsController> WebApplicationFactory;
    public HttpClient Client;
    public IPaymentsRepository PaymentsRepository;
    public Mock<HttpMessageHandler> MockHttpMessageHandler;
    public IPaymentsService PaymentsService;

    public PaymentsControllerTestFixture()
    {
        var mockLogger = new Mock<ILogger<IPaymentsRepository>>();
        PaymentsRepository = new PaymentsRepository(mockLogger.Object);

        MockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(MockHttpMessageHandler.Object);
        var mockServiceLogger = new Mock<ILogger<IPaymentsService>>();
        PaymentsService = new PaymentsService(PaymentsRepository, httpClient, mockServiceLogger.Object);

        WebApplicationFactory = new WebApplicationFactory<PaymentsController>();

        Client = WebApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IPaymentsRepository>(PaymentsRepository);
                    services.AddSingleton<IPaymentsService>(PaymentsService);
                }))
            .CreateClient();
    }
    
    public void SetupBankSimulatorMockResponse(HttpStatusCode statusCode, bool authorized, Guid? authorizationCode = null)
    {
        MockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = JsonContent.Create(new BankSimulatorResponse
                {
                    StatusCode = (int)statusCode,
                    Authorized = authorized,
                    AuthorizationCode = authorizationCode
                })
            });
    }

    public void Dispose()
    {
        WebApplicationFactory?.Dispose();
        Client?.Dispose();
    }
}