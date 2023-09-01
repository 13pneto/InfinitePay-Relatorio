using InfinitePay.Responses;
using Refit;

namespace InfinitePay;

[Headers("User-Agent: Refit")]
public interface IInfinitePayClient
{
    [Get("/banking/transactions?to_date={dateTo}&from_date={dateFrom}&limit=50000")]
    Task<AllTransactionsResponse> GetTransactions(string dateFrom, string dateTo);
    
    
    [Get("/banking/transactions/{transactionId}")]
    Task<TransactionDetailsResponse> GetTransactionDetails(string transactionId);
}