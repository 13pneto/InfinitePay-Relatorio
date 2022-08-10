namespace InfinitePay.Responses;

public class InstallmentsDetailResponse
{
    public int amount { get; set; }
    public double mdr_fee { get; set; }
    public double anticipation_fee { get; set; }
    public DateTime payment_date { get; set; }
    public DateTime predicted_payment_date { get; set; }
    public double predicted_anticipation_fee { get; set; }

}