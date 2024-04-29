using System.Text.Json;
using System.Text.RegularExpressions;
using Mexc.Sdk;

class Program
{
    static void Main(string[] args)
    {
        //mexc
        string apiKey = "InsertMEXCAPIKey";
        string apiSecret = "InsertAPISecret";
        var spot = new Spot(apiKey, apiSecret);
        // Fetch account information
        string? accountInfo = GetAccountInfo(spot);
        //Console.WriteLine(accountInfo);
        // Fetch asset values and calculate holdings
        double? spotHolding = CalculateHoldingsMEXC(spot, accountInfo);

        Console.WriteLine($"Your MEXC Spot Holding: ${spotHolding}");

        //Example
        double Bitcoin = GetQuote(spot, "BTCUSDT", 1);
        Console.WriteLine($"Bitcoin Price: ${Bitcoin}");
    }

    static string? GetAccountInfo(Spot spot)
    {
        try
        {
            return JsonSerializer.Serialize(spot.AccountInfo());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching account information: {ex.Message}");
            return default;
        }
    }

    static double CalculateHoldingsMEXC(Spot spot, string accountInfo)
    {
        // Match asset balances using regex pattern
        string pattern = "\\{\"asset\":\"(\\w+)\",\"free\":\"([\\d.]+)\",\"locked\":\"([\\d.]+)\"";
        MatchCollection matches = Regex.Matches(accountInfo, pattern);

        double spotHolding = 0;

        foreach (Match match in matches)
        {
            string assetTicker = match.Groups[1].Value;
            double freeAmount = double.Parse(match.Groups[2].Value);
            double lockedAmount = double.Parse(match.Groups[3].Value);
            double assetValue = GetQuote(spot, assetTicker, 1);

            // Calculate holdings
            spotHolding += (assetValue * freeAmount) + (assetValue * lockedAmount);
        }

        return spotHolding;
    }

    static double GetQuote(Spot spot, string symbol, double amount)
    {
        try
        {
            if (symbol == "USDT")
            {
                return 1.0;
            }

            if (!symbol.EndsWith("USDT"))
            {
                symbol += "USDT";
            }

            string mexcQuote = JsonSerializer.Serialize(spot.TickerPrice(symbol));

            if (mexcQuote.Contains(symbol))
            {
                Match match = Regex.Match(mexcQuote, "\"price\":\"(\\d+\\.\\d+)\"");
                if (match.Success && double.TryParse(match.Groups[1].Value, out double lastPrice))
                {
                    return amount * lastPrice;
                }

                Console.WriteLine($"Price not found for {symbol}");
            }
            else
            {
                Console.WriteLine($"Symbol not found: {symbol}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting quote for {symbol}: {ex.Message}");
        }

        return 0.0;
    }
}
