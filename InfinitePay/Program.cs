// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using InfinitePay;
using InfinitePay.Responses;
using Refit;



var infinitePayClient = RestService
    .For<IInfinitePayClient>(new HttpClient(new InfinitePayAuthenticationHandler())
    {
        BaseAddress = new Uri("https://api.infinitepay.io")
    });

DateOnly dateFrom = default;
DateOnly dateTo = default;
var now = DateTime.Now;

    try
    {
        var isValidAuth = false;
        while (isValidAuth == false)
        {
            Console.WriteLine("Informe o authorization:");
            var authorization = Console.ReadLine();
            
            isValidAuth = authorization.Length > 10;

            Auth.Token = authorization;
        }
        
        var isValidDateFrom = false;
        while (isValidDateFrom == false)
        {
            Console.WriteLine("Data inicio: ex:1/1/22 (DEIXE VAZIO PARA MÊS ANTERIOR)");
            var dateFromString = Console.ReadLine();

            if (dateFromString is {Length: 0})
            {
                dateFrom = new DateOnly(now.Year, now.Month == 1 ? 12 : now.Month - 1, 01);
                isValidDateFrom = true;
            }
            else
            {
                isValidDateFrom = DateOnly.TryParse(dateFromString, out dateFrom);   
            }
        }
        
        var isValidDateTo = false;
        while (isValidDateTo == false)
        {
            Console.WriteLine("Data final: ex: 1/2/22 (DEIXE VAZIO PARA MÊS ATUAL)");
            var dateToString = Console.ReadLine();

            if (dateToString is {Length: 0})
            {
                dateTo = new DateOnly(now.Year, now.Month, 01);
                isValidDateTo = true;
            }
            else
            {
                isValidDateTo = DateOnly.TryParse(dateToString, out dateTo);
            }
        }
	
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }


var dateFromParameter =
    (new DateTimeOffset(dateFrom.Year, dateFrom.Month, dateFrom.Day, 0, 0, 0, TimeSpan.FromHours(-3)))
    .ToString("O");
var dateToParameter = 
    (new DateTimeOffset(dateTo.Year, dateTo.Month, dateTo.Day, 0, 0, 0, TimeSpan.FromHours(-3)))
    .ToString("O");

	Console.WriteLine(dateFromParameter);
	Console.WriteLine(dateToParameter);


var transactionsResponse =
    await infinitePayClient.GetTransactions(dateFromParameter, dateToParameter);


var transactionsIds = transactionsResponse.results
    .Where(x => x.status == "approved")
    .OrderBy(x => x.created_at)
    .Select(x => x.nsu);

var transactionsDetails = new List<TransactionDetailsResponse>();

foreach (var transactionsId in transactionsIds)
{
    var transactionDetailsResponse = await infinitePayClient.GetTransactionDetails(transactionsId);
    transactionsDetails.Add(transactionDetailsResponse);
}

var folderPath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Infinitepay";
if (Directory.Exists(folderPath) == false)
{
    Directory.CreateDirectory(folderPath);
}

var fileName =
    @$"{folderPath}\relatorio-infinitepay-de{dateFrom.Day}-{dateFrom.Month}-{dateFrom.Year}" +
    $"-ate-{dateTo.Day}-{dateTo.Month}-{dateTo.Year}.html";

using (var stringWriter = new StreamWriter(fileName))
{
    stringWriter.Write("<html>");
    stringWriter.Write("	<head>");
    ;
    stringWriter.Write(
        "		<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/bootstrap@4.1.3/dist/css/bootstrap.min.css\" integrity=\"sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO\" crossorigin=\"anonymous\"");
    stringWriter.Write("	</head>");
    stringWriter.Write("");
    stringWriter.Write("	<body>");
    
    
    stringWriter.Write("<div class=\"alert alert-success\" role=\"alert\">");
    stringWriter.Write($"Periodo: {dateFrom.ToString("dd/MM/yyyy")} - {dateTo.ToString("dd/MM/yyyy")}");
    stringWriter.Write("</div>");


    stringWriter.Write("<table class=\"table table-bordered\">");
    stringWriter.Write("<thead>");
    stringWriter.Write("<tr>");
    stringWriter.Write("<th scope=\"col\">Cliente</th>");
    stringWriter.Write("<th scope=\"col\">Bandeira</th>");
    stringWriter.Write("<th scope=\"col\">Metodo</th>");
    stringWriter.Write("<th scope=\"col\">Data</th>");
    stringWriter.Write("<th scope=\"col\">Valor cobrado R$</th>");
    stringWriter.Write("<th scope=\"col\">Valor recebido R$</th>");
    stringWriter.Write("<th scope=\"col\">Tarifa %</th>");
    stringWriter.Write("<th scope=\"col\">Tarifa R$</th>");
    stringWriter.Write("</tr>");
    stringWriter.Write("</thead>");

    stringWriter.Write("<tbody>");

    var percentages = new List<decimal>();
    
    foreach (var transactionDetailResponse in transactionsDetails)
    {
        var taxPercentage = (transactionDetailResponse.tax_amount / transactionDetailResponse.net_amount) * 100;
        percentages.Add(taxPercentage);
        
        stringWriter.Write("<tr>");

        stringWriter.Write($"<td>{transactionDetailResponse.buyer_details.name}");
        stringWriter.Write("</td>");

        stringWriter.Write($"<td>{transactionDetailResponse.buyer_details.card_brand}");
        stringWriter.Write("</td>");

        var paymentMethod = transactionDetailResponse.payment_method == "debit"
            ? "Débito"
            : transactionDetailResponse.payment_method == "credit"
                ? "Crédito"
                : "";
        
        stringWriter.Write($"<td>{paymentMethod}");
        stringWriter.Write("</td>");

        stringWriter.Write($"<td>{transactionDetailResponse.created_at.Date}");
        stringWriter.Write("</td>");

        stringWriter.Write($"<td>R$ {transactionDetailResponse.amount.ToString("N3")}");
        stringWriter.Write("</td>");

        stringWriter.Write($"<td>R$ {transactionDetailResponse.net_amount.ToString("N3")}");
        stringWriter.Write("</td>");

        stringWriter.Write($"<td>{taxPercentage.ToString("N3")} %");
        stringWriter.Write("</td>");

        stringWriter.Write($"<td>R$ {transactionDetailResponse.tax_amount.ToString("N3")}");
        stringWriter.Write("</td>");

        stringWriter.Write("</tr>");
    }

    var totalReceipt = transactionsDetails.Sum(x => x.net_amount).ToString("N3");
    var totalCharged = transactionsDetails.Sum(x => x.amount).ToString("N3");
    var totalTax = transactionsDetails.Sum(x => x.tax_amount).ToString("N3");

    var averageTaxPercentage = percentages.Average().ToString("N3");

    stringWriter.Write("<tr>");
    stringWriter.Write($"<th colspan=\"4\">TOTAL - {transactionsDetails.Count}");
    stringWriter.Write("</th>");
    stringWriter.Write($"<th>R$ {totalCharged}");
    stringWriter.Write("</th>");
    stringWriter.Write($"<th>R$ {totalReceipt}");
    stringWriter.Write("</th>");
    stringWriter.Write($"<th>~ {averageTaxPercentage} %");
    stringWriter.Write("</th>");
    stringWriter.Write($"<th>R$ {totalTax}");
    stringWriter.Write("</th>");
    stringWriter.Write("</tr>");


    stringWriter.Write("</tbody>");
    stringWriter.Write("</table>");
    
    
    stringWriter.Write("<hr> </hr>");

    stringWriter.Write("<div class=\"row\">");
    
    stringWriter.Write("<div class=\"col-6\">");

    stringWriter.Write("<table class=\"table table-bordered\">");
    stringWriter.Write("<thead>");
    stringWriter.Write("<tr>");
    stringWriter.Write("<th scope=\"col\">Dia</th>");
    stringWriter.Write("<th scope=\"col\">Total bruto</th>");
    stringWriter.Write("<th scope=\"col\">Total liquido</th>");
    stringWriter.Write("<th scope=\"col\">Total taxas</th>");
    stringWriter.Write("</tr>");
    stringWriter.Write("</thead>");

    stringWriter.Write("<tbody>");

    for (int i = 1; i <= 31; i++)
    {
        stringWriter.Write("<tr>");

       
        var numberString = i.ToString();
        if (i < 10)
        {
            numberString = 0 + numberString;
        }
        
        var totalLiquido = transactionsDetails
            .Where(x => x.created_at.ToString().StartsWith(numberString))
            .Sum(x => x.net_amount);

        var totalBruto = transactionsDetails
            .Where(x => x.created_at.ToString().StartsWith(numberString))
            .Sum(x => x.amount);

        stringWriter.Write($"<td>{numberString}");
        stringWriter.Write("</td>");

        stringWriter.Write($"<td> R$ {totalBruto.ToString("N3")}");
        stringWriter.Write("</td>");

        stringWriter.Write($"<td> R$ {totalLiquido.ToString("N3")}");
        stringWriter.Write("</td>");

        var diff = totalBruto - totalLiquido;
        
        stringWriter.Write($"<td> R$ {diff.ToString("N3")}");
        stringWriter.Write("</td>");
    }
    stringWriter.Write("</tbody>");
    
    stringWriter.Write("</div>");
    stringWriter.Write("</table>");
    stringWriter.Write("</div>");

    


    stringWriter.Write("	</body>");
    stringWriter.Write("</html>");
}

var p = new Process();
p.StartInfo = new ProcessStartInfo(fileName)
{
    UseShellExecute = true
};
p.Start();