// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using InfinitePay;
using InfinitePay.Responses;
using Refit;


var infinitePayClient = RestService.For<IInfinitePayClient>("https://api.infinitepay.io");

string authorization = null;

string dayFrom = null;
string monthFrom = null;
string yearFrom = null;

string dayTo = null;
string monthTo = null;
string yearTo = null;

var isValid = false;

while (isValid == false)
{
    try
    {
        Console.WriteLine("Informe o authorization:");
        authorization = Console.ReadLine();

        Console.WriteLine("Data inicio: ex: 01/01/2022");
        var dateFrom = Console.ReadLine();

        Console.WriteLine("Data final: ex: 31/01/2022");
        var dateTo = Console.ReadLine();

        var dateFromSplit = dateFrom.Split('/');
        dayFrom = dateFromSplit[0];
        monthFrom = dateFromSplit[1];
        yearFrom = dateFromSplit[2];

        var dateToSplit = dateTo.Split('/');
        dayTo = dateToSplit[0];
        monthTo = dateToSplit[1];
        yearTo = dateToSplit[2];

        isValid = true;
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}


var dateFromParameter = $"{yearFrom}-{monthFrom}-{dayFrom}";
var dateToParameter = $"{yearTo}-{monthTo}-{dayTo}";

var transactionsResponse =
    await infinitePayClient.GetTransactions(dateFromParameter, dateToParameter, authorization);


var transactionsIds = transactionsResponse.results
    .Where(x => x.status == "approved")
    .OrderBy(x => x.created_at)
    .Select(x => x.nsu);

var transactionsDetails = new List<TransactionDetailsResponse>();

foreach (var transactionsId in transactionsIds)
{
    var transactionDetailsResponse = await infinitePayClient.GetTransactionDetails(transactionsId, authorization);
    transactionsDetails.Add(transactionDetailsResponse);
}

var folderPath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Infinitepay";
if (Directory.Exists(folderPath) == false)
{
    Directory.CreateDirectory(folderPath);
}

var fileName =
    @$"{folderPath}\relatorio-infinitepay-de{dayFrom}-{monthFrom}-{yearFrom}" +
    $"-ate-{dayTo}-{monthTo}-{yearTo}.html";

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
    stringWriter.Write($"Periodo: {dayFrom}/{monthFrom}/{yearFrom} - {dayTo}/{monthTo}/{yearTo}");
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
    stringWriter.Write($"<th>~ {averageTaxPercentage}");
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
    stringWriter.Write("<th scope=\"col\">Total Liquido</th>");
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
            .Sum(x => x.net_amount)
            .ToString("N3");

        var totalBruto = transactionsDetails
            .Where(x => x.created_at.ToString().StartsWith(numberString))
            .Sum(x => x.amount)
            .ToString("N3");

        stringWriter.Write($"<td>{numberString}");
        stringWriter.Write("</td>");

        stringWriter.Write($"<td>{totalBruto}");
        stringWriter.Write("</td>");

        stringWriter.Write($"<td>{totalLiquido}");
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