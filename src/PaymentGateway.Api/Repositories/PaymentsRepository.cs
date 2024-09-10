using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Repositories;

public class PaymentsRepository : IPaymentsRepository
{
    public List<PostPaymentResponse> Payments = new();
    private readonly ILogger<IPaymentsRepository> _logger;

    public PaymentsRepository(ILogger<IPaymentsRepository> logger)
    {
        _logger = logger;
    }

    public void Add(PostPaymentResponse payment)
    {
        Payments.Add(payment);
    }

    public PostPaymentResponse Get(Guid id)
    {
        var payments = Payments.FirstOrDefault(p => p.Id == id);

        if (payments is null)
        {
            _logger.LogWarning("Payment with ID {PaymentId} not found", id);
            throw new KeyNotFoundException($"Payment with ID {id} not found.");
        }

        return payments;
    }
}