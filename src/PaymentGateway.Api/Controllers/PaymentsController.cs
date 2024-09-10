using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IPaymentsService _paymentsService;

    public PaymentsController(IPaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsService.Get(id);

        return new OkObjectResult(payment);
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> ProcessPaymentAsync(
        [FromBody] PostPaymentRequest paymentRequest)
    {
        PostPaymentRequestValidator validator = new();

        var validatorResult = await validator.ValidateAsync(paymentRequest);

        if (validatorResult.IsValid)
        {
            return new BadRequestObjectResult(new { PaymentStatus.Rejected, validatorResult });
        }

        var paymentResponse = await _paymentsService.ProcessPayment(paymentRequest);
        
        return paymentResponse.Status != PaymentStatus.Authorized
            ? StatusCode(StatusCodes.Status402PaymentRequired, paymentResponse)
            : new OkObjectResult(paymentResponse);
    }
}