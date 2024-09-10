using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories;

using Guid = System.Guid;

namespace PaymentGateway.Api.Services;

public class PaymentsService : IPaymentsService
{
    private readonly IPaymentsRepository _paymentsRepository;

    public PaymentsService(IPaymentsRepository paymentsRepository)
    {
        _paymentsRepository = paymentsRepository;
    }

    public PostPaymentResponse Get(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<PostPaymentResponse> ProcessPayment(PostPaymentRequest paymentRequest)
    {
        //Forward payment request to acquiring bank(bank_simulator.js)
        HttpClient httpClient = new();

        var bankSimulatorRequest = new
        {
            card_number = paymentRequest.CardNumber.ToString(),
            expiry_date = $"{paymentRequest.ExpiryMonth}/{paymentRequest.ExpiryYear}",
            currency = paymentRequest.Currency,
            amount = paymentRequest.Amount,
            cvv = paymentRequest.Cvv
        };

        var response = await httpClient.PostAsJsonAsync("http://localhost:8080/payments", bankSimulatorRequest);
        BankSimulatorResponse bankSimulatorResponse =
            (await response.Content.ReadFromJsonAsync<BankSimulatorResponse>())!;


        if (bankSimulatorResponse.StatusCode is 400 || bankSimulatorResponse.Authorized is false)
        {
            PostPaymentResponse paymentResponse = new() { Status = PaymentStatus.Declined };

            return paymentResponse;
        }


        PostPaymentResponse authorizedPayment = new()
        {
            Id = new Guid(),
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = paymentRequest.CardNumber % 10000,
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear,
            Currency = paymentRequest.Currency,
            Amount = paymentRequest.Amount
        };
        _paymentsRepository.Add(authorizedPayment);
        return authorizedPayment;
    }
}