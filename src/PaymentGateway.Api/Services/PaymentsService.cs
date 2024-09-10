using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories;

using Guid = System.Guid;

namespace PaymentGateway.Api.Services;

public class PaymentsService : IPaymentsService
{
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly HttpClient _client;

    public PaymentsService(IPaymentsRepository paymentsRepository, HttpClient client)
    {
        _paymentsRepository = paymentsRepository;
        _client = client;
    }

    public PostPaymentResponse Get(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<PostPaymentResponse> ProcessPayment(PostPaymentRequest paymentRequest)
    {
        BankSimulatorResponse bankSimulatorResponse = await ForwardPaymentToAcquiringBank(paymentRequest);

        if (!IsPaymentAuthorized(bankSimulatorResponse))
        {
            return new PostPaymentResponse { Status = PaymentStatus.Declined };
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

    private async Task<BankSimulatorResponse> ForwardPaymentToAcquiringBank(PostPaymentRequest postPaymentRequest)
    {
        var bankSimulatorRequest = new
        {
            card_number = postPaymentRequest.CardNumber.ToString(),
            expiry_date = $"{postPaymentRequest.ExpiryMonth}/{postPaymentRequest.ExpiryYear}",
            currency = postPaymentRequest.Currency,
            amount = postPaymentRequest.Amount,
            cvv = postPaymentRequest.Cvv
        };

        var response = await _client.PostAsJsonAsync("http://localhost:8080/payments", bankSimulatorRequest);
        BankSimulatorResponse bankSimulatorResponse =
            (await response.Content.ReadFromJsonAsync<BankSimulatorResponse>())!;

        return bankSimulatorResponse;
    }

    private bool IsPaymentAuthorized(BankSimulatorResponse bankSimulatorResponse1)
    {
        return bankSimulatorResponse1.StatusCode != 400 && bankSimulatorResponse1.Authorized is true;
    }
}