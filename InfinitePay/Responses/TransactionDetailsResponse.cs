namespace InfinitePay.Responses{ 

    public class TransactionDetailsResponse
    {
        public string event_type { get; set; }
        public string payment_method { get; set; }
        public string merchant_id { get; set; }
        public string serial_number { get; set; }
        public string status { get; set; }
        public string nsu { get; set; }
        public string event_reference_id { get; set; }
        public int installments { get; set; }
        public List<InstallmentsDetailResponse> installments_details { get; set; }
        public int amount { get; set; }
        public double net_amount { get; set; }
        public double tax_amount { get; set; }
        public DateTime created_at { get; set; }
        public BuyerDetailsResponse buyer_details { get; set; }

    }

}