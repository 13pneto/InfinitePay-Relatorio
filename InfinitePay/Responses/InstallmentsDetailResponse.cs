namespace InfinitePay.Responses;

public class InstallmentsDetailResponse
{
    public decimal amount { get; set; }
    public double mdr_fee { get; set; }
    public decimal anticipation_fee { get; set; }
    public DateTime payment_date { get; set; }
    public DateTime predicted_payment_date { get; set; }
    public decimal predicted_anticipation_fee { get; set; }

}