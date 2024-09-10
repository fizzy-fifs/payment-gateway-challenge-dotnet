using FluentValidation;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators;

public class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
    public PostPaymentRequestValidator()
    {
        RuleFor(p => p.CardNumber)
            .Must(cn => cn.ToString().Length >= 14 && cn.ToString().Length <= 19)
            .WithMessage("Card number must be between 14 to 19 digits long");

        RuleFor(p => p)
            .Must(BeAValidExpiryDate)
            .WithMessage("Please enter a valid expiry date");

        RuleFor(p => p.Currency)
            .Must(curr => curr.Length.Equals(3));

        RuleFor(p => p.Cvv)
            .Must(cvv => cvv.ToString().Length >= 3 && cvv.ToString().Length <= 4);
    }

    private bool BeAValidExpiryDate(PostPaymentRequest paymentRequest)
    {
        if (paymentRequest.ExpiryMonth is < 1 or > 12)
        {
            return false;
        }

        var expiryDate = new DateTime(paymentRequest.ExpiryYear, paymentRequest.ExpiryMonth,
            DateTime.DaysInMonth(paymentRequest.ExpiryYear, paymentRequest.ExpiryMonth));

        return expiryDate > DateTime.Now;
    }
}