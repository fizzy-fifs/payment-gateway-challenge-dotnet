using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentsService
{
    PostPaymentResponse Get(Guid id);
    Task<PostPaymentResponse> ProcessPayment(PostPaymentRequest paymentRequest);
}