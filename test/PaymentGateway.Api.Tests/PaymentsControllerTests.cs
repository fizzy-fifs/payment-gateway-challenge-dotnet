using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests : IClassFixture<PaymentsControllerTestFixture>
{
    private readonly Random _random = new();
    private readonly PaymentsControllerTestFixture _fixture;

    public PaymentsControllerTests(PaymentsControllerTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP"
        };

        _fixture.PaymentsRepository.Add(payment);

        // Act
        var response = await _fixture.Client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ProcessesAValidPaymentSuccessfully()
    {
        // Arrange
        _fixture.SetupBankSimulatorMockResponse(HttpStatusCode.OK, true, Guid.NewGuid());
        
        PostPaymentRequest paymentRequest = new()
        {
            ExpiryYear = 2026,
            ExpiryMonth = 10,
            Amount = 1000,
            CardNumber = 2098345812973645,
            Currency = "GBP",
            Cvv = 123
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync($"/api/Payments/process", paymentRequest);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(paymentResponse!.ExpiryYear, paymentRequest.ExpiryYear);
        Assert.Equal(paymentResponse!.ExpiryMonth, paymentRequest.ExpiryMonth);
        Assert.Equal(paymentResponse!.Amount, paymentRequest.Amount);
        Assert.Equal(paymentResponse!.CardNumberLastFour, Int32.Parse(paymentRequest.CardNumber.ToString()[^4..]));
        Assert.Equal(paymentResponse!.Currency, paymentRequest.Currency);
    }
}