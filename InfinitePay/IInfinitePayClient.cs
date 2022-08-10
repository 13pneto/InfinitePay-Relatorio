using InfinitePay.Responses;
using Refit;

namespace InfinitePay;

public interface IInfinitePayClient
{
    [Get("/banking/transactions?limit=50000&from_date={dateFrom}T03%3A00%3A00.000Z&to_date={dateTo}T02%3A59%3A59.999Z")]
    Task<AllTransactionsResponse> GetTransactions(string dateFrom, string dateTo, [Header("authorization")] string authorization);
    
    
    [Get("/banking/transactions/{transactionId}")]
    Task<TransactionDetailsResponse> GetTransactionDetails(string transactionId, [Header("authorization")] string authorization);
}