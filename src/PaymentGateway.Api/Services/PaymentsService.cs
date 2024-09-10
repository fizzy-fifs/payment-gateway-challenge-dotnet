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
    private readonly ILogger<IPaymentsService> _logger;

    public PaymentsService(IPaymentsRepository paymentsRepository, HttpClient client, ILogger<IPaymentsService> logger)
    {
        _paymentsRepository = paymentsRepository;
        _client = client;
        _logger = logger;
    }

    public PaymentSearchResult Get(Guid id)
    {
        try
        {
            var payments = _paymentsRepository.Get(id);
            GetPaymentResponse paymentResponse =  new()
            {
                Id = payments.Id,
                Status = payments.Status,
                CardNumberLastFour = payments.CardNumberLastFour,
                ExpiryMonth = payments.ExpiryMonth,
                ExpiryYear = payments.ExpiryYear,
                Currency = payments.Currency,
                Amount = payments.Amount
            };

            return new PaymentSearchResult()
            {
                SearchStatus = PaymentSearchStatus.Found,
                Payment = paymentResponse
            };
        }
        catch (KeyNotFoundException ex)
        {
            return new PaymentSearchResult() { SearchStatus = PaymentSearchStatus.NotFound };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving payment with ID {PaymentId}", id);
            return new PaymentSearchResult() { SearchStatus = PaymentSearchStatus.Error };
        }
    }

    public async Task<PostPaymentResponse> ProcessPayment(PostPaymentRequest paymentRequest)
    {
        var bankSimulatorResponse = await ForwardPaymentToAcquiringBank(paymentRequest);

        if (bankSimulatorResponse is null || !IsPaymentAuthorized(bankSimulatorResponse))
        {
            return new PostPaymentResponse { Status = PaymentStatus.Declined };
        }

        PostPaymentResponse authorizedPayment = new()
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = Int32.Parse(paymentRequest.CardNumber.ToString()[^4..]),
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear,
            Currency = paymentRequest.Currency,
            Amount = paymentRequest.Amount
        };
        _paymentsRepository.Add(authorizedPayment);

        return authorizedPayment;
    }

    private async Task<BankSimulatorResponse?> ForwardPaymentToAcquiringBank(PostPaymentRequest postPaymentRequest)
    {
        var bankSimulatorRequest = new
        {
            card_number = postPaymentRequest.CardNumber.ToString(),
            expiry_date = $"{postPaymentRequest.ExpiryMonth:D2}/{postPaymentRequest.ExpiryYear}",
            currency = postPaymentRequest.Currency,
            amount = postPaymentRequest.Amount,
            cvv = postPaymentRequest.Cvv
        };

        var response = await _client.PostAsJsonAsync("http://localhost:8080/payments", bankSimulatorRequest);
        var bankSimulatorResponse =
            await response.Content.ReadFromJsonAsync<BankSimulatorResponse>();

        return bankSimulatorResponse;
    }

    private bool IsPaymentAuthorized(BankSimulatorResponse bankSimulatorResponse)
    {
        return bankSimulatorResponse.StatusCode != 400 && bankSimulatorResponse.Authorized is true;
    }
}

public class PaymentSearchResult
{
    public required PaymentSearchStatus SearchStatus { get; set; }
    public GetPaymentResponse Payment { get; set; }
}

