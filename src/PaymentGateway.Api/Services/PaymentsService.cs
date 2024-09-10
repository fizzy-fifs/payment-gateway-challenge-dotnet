using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsService : IPaymentsService
{
    public PostPaymentResponse Get(Guid id)
    {
        throw new NotImplementedException();
    }

    public PostPaymentResponse ProcessPayments(PostPaymentRequest paymentRequest)
    {
        throw new NotImplementedException();
    }
}