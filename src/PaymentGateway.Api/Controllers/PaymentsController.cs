using FluentValidation.Results;

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
    public async Task<ActionResult<GetPaymentResponse>> GetPaymentAsync(Guid id)
    {
        var paymentSearchResult = _paymentsService.Get(id);

        return paymentSearchResult.SearchStatus switch
        {
            PaymentSearchStatus.Found => Ok(paymentSearchResult.Payment),
            PaymentSearchStatus.NotFound => NotFound("Payment not found"),
            PaymentSearchStatus.Error => StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the payment"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpPost("process")]
    public async Task<ActionResult<PostPaymentResponse>> ProcessPaymentAsync(
        [FromBody] PostPaymentRequest paymentRequest)
    {
        ValidationResult validatorResult = await ValidatePaymentRequestAsync(paymentRequest);

        if (!validatorResult.IsValid)
        {
            return new BadRequestObjectResult(new { PaymentStatus.Rejected, validatorResult });
        }

        var paymentResponse = await _paymentsService.ProcessPayment(paymentRequest);

        return paymentResponse.Status switch
        {
            PaymentStatus.Authorized => Ok(paymentResponse),
            PaymentStatus.Declined => StatusCode(StatusCodes.Status402PaymentRequired, paymentResponse),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private async Task<ValidationResult> ValidatePaymentRequestAsync(PostPaymentRequest postPaymentRequest)
    {
        PostPaymentRequestValidator validator = new();
        return await validator.ValidateAsync(postPaymentRequest);
    }
}