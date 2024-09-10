namespace PaymentGateway.Api.Models.Requests;

public class BankSimulatorResponse
{
    public int StatusCode { get; set; }
    public bool? Authorized { get; set; }
    public Guid? AuthorizationCode { get; set; }
    public string? ErrorMessage { get; set; }
}