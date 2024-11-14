using Newtonsoft.Json.Linq;

public class StudentLoanCalculator
{
    private const double RepaymentThreshold = 28470; // Plan 2 threshold
    private const double RepaymentRate = 0.09; // 9% of earnings above the threshold
    private const int LoanTermYears = 30; // Loan term until write-off

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Choose calculation mode:");
        Console.WriteLine("1: Early repayment comparison (1, 5, 10 years)");
        Console.WriteLine("2: Total savings if loan paid off today");
        int mode = int.Parse(Console.ReadLine());

        string apiKey = Environment.GetEnvironmentVariable("API_NINJAS_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("API key is missing. Please set the 'API_NINJAS_KEY' environment variable.");
            return;
        }

        // Common Inputs
        Console.Write("Enter your annual wage: £");
        double annualWage = double.Parse(Console.ReadLine());

        Console.Write("Enter your loan balance: £");
        double loanBalance = double.Parse(Console.ReadLine());

        Console.Write("Enter expected annual wage increase rate (e.g., 0.03 for 3%): ");
        double wageIncreaseRate = double.Parse(Console.ReadLine());

        // Fetch the latest interest rate + 3%
        double interestRate = await FetchInterestRateAsync(apiKey);
        Console.WriteLine($"Latest interest rate with +3%: {interestRate}%");

        if (mode == 1)
        {
            // Early repayment comparison
            double totalRepaymentStandard = CalculateTotalRepayment(annualWage, loanBalance, wageIncreaseRate, interestRate, out double monthlyRepaymentStandard);
            CalculateEarlyRepaymentSavings(loanBalance, annualWage, wageIncreaseRate, interestRate, monthlyRepaymentStandard);
        }
        else if (mode == 2)
        {
            // Early payoff savings comparison
            Console.Write("Enter how many years you have left on the loan: ");
            int yearsLeft = int.Parse(Console.ReadLine());

            CalculatePayoffTodaySavings(loanBalance, annualWage, wageIncreaseRate, interestRate, yearsLeft);
        }
    }

    private static double CalculateTotalRepayment(double annualWage, double loanBalance, double wageIncreaseRate, double interestRate, out double monthlyRepayment)
    {
        double totalRepayment = 0;
        double annualRepaymentSum = 0;

        for (int year = 1; year <= LoanTermYears; year++)
        {
            double applicableInterestRate = CalculateInterestRate(annualWage, interestRate);
            loanBalance *= (1 + applicableInterestRate);
            Console.WriteLine($"\nYear {year}:");
            Console.WriteLine($"  Balance after interest ({applicableInterestRate * 100:F2}%): £{loanBalance:F2}");

            double repayableIncome = Math.Max(0, annualWage - RepaymentThreshold);
            double annualRepayment = repayableIncome * RepaymentRate;
            loanBalance -= annualRepayment;
            totalRepayment += annualRepayment;
            annualRepaymentSum += annualRepayment;

            Console.WriteLine($"  Annual repayment based on income: £{annualRepayment:F2}");
            Console.WriteLine($"  Remaining balance after repayment: £{loanBalance:F2}");

            if (loanBalance <= 0)
            {
                Console.WriteLine("Loan fully repaid.");
                break;
            }

            annualWage *= (1 + wageIncreaseRate);
        }

        monthlyRepayment = annualRepaymentSum / (LoanTermYears * 12);
        return totalRepayment;
    }

    private static void CalculateEarlyRepaymentSavings(double loanBalance, double annualWage, double wageIncreaseRate, double interestRate, double monthlyRepaymentStandard)
    {
        int[] earlyPayoffYears = { 1, 5, 10 };

        Console.WriteLine("\nEarly Repayment Scenarios:");
        foreach (int payoffYear in earlyPayoffYears)
        {
            double totalRepaymentEarly = 0;
            double balance = loanBalance;
            double wage = annualWage;

            for (int year = 1; year <= payoffYear; year++)
            {
                double applicableInterestRate = CalculateInterestRate(wage, interestRate);
                balance *= (1 + applicableInterestRate);
                Console.WriteLine($"\nYear {year} (Early Payoff after {payoffYear} years):");
                Console.WriteLine($"  Balance after interest ({applicableInterestRate * 100:F2}%): £{balance:F2}");

                double repayableIncome = Math.Max(0, wage - RepaymentThreshold);
                double annualRepayment = repayableIncome * RepaymentRate;
                balance -= annualRepayment;
                totalRepaymentEarly += annualRepayment;

                Console.WriteLine($"  Annual repayment based on income: £{annualRepayment:F2}");
                Console.WriteLine($"  Remaining balance after repayment: £{balance:F2}");

                if (balance <= 0)
                {
                    Console.WriteLine("Loan fully repaid early.");
                    break;
                }

                wage *= (1 + wageIncreaseRate);
            }

            double monthlyRepaymentEarly = totalRepaymentEarly / (payoffYear * 12);
            double monthlySavings = monthlyRepaymentStandard - monthlyRepaymentEarly;
            Console.WriteLine($"\nSummary for Early Payoff after {payoffYear} years:");
            Console.WriteLine($"  Total repayment: £{totalRepaymentEarly:F2}");
            Console.WriteLine($"  Monthly repayment: £{monthlyRepaymentEarly:F2}");
            Console.WriteLine($"  Monthly savings compared to standard: £{monthlySavings:F2}");
        }
    }

    private static void CalculatePayoffTodaySavings(double loanBalance, double annualWage, double wageIncreaseRate, double interestRate, int yearsLeft)
    {
        double totalRepaymentOverTime = 0;
        double remainingBalance = loanBalance;

        for (int year = 1; year <= yearsLeft; year++)
        {
            double applicableInterestRate = CalculateInterestRate(annualWage, interestRate);
            remainingBalance *= (1 + applicableInterestRate);

            Console.WriteLine($"\nYear {year} (Remaining Years to Pay Off):");
            Console.WriteLine($"  Balance after interest ({applicableInterestRate * 100:F2}%): £{remainingBalance:F2}");

            double repayableIncome = Math.Max(0, annualWage - RepaymentThreshold);
            double annualRepayment = repayableIncome * RepaymentRate;
            remainingBalance -= annualRepayment;
            totalRepaymentOverTime += annualRepayment;

            Console.WriteLine($"  Annual repayment based on income: £{annualRepayment:F2}");
            Console.WriteLine($"  Remaining balance after repayment: £{remainingBalance:F2}");

            if (remainingBalance <= 0)
            {
                Console.WriteLine("Loan fully repaid.");
                break;
            }

            annualWage *= (1 + wageIncreaseRate);
        }

        double interestSavings = totalRepaymentOverTime - loanBalance;
        double monthlySavingsValue = (totalRepaymentOverTime / (yearsLeft * 12)) * yearsLeft * 12;

        Console.WriteLine("\nIf you paid off the loan today:");
        Console.WriteLine($"  Total repayment over remaining years: £{totalRepaymentOverTime:F2}");
        Console.WriteLine($"  Interest savings if paid off today: £{interestSavings:F2}");
        Console.WriteLine($"  Future value of saved monthly repayments over remaining term: £{monthlySavingsValue:F2}");
    }

    private static double CalculateInterestRate(double annualWage, double baseInterestRate)
    {
        const double lowerThreshold = 28470;
        const double upperThreshold = 51245;
        const double maxAdditionalRate = 0.03; // 3%

        if (annualWage <= lowerThreshold)
        {
            return baseInterestRate / 100;
        }
        else if (annualWage >= upperThreshold)
        {
            return (baseInterestRate / 100) + maxAdditionalRate;
        }
        else
        {
            double additionalRate = ((annualWage - lowerThreshold) / (upperThreshold - lowerThreshold)) * maxAdditionalRate;
            return (baseInterestRate / 100) + additionalRate;
        }
    }

    private static async Task<double> FetchInterestRateAsync(string ApiKey)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);
            string url = "https://api.api-ninjas.com/v1/interestrate?country=United Kingdom";

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseBody);
                double baseRate = json["central_bank_rates"][0]["rate_pct"].Value<double>();

                return baseRate + 3.0;
            }
            else
            {
                Console.WriteLine("Failed to retrieve interest rate data.");
                return 4.3 + 3.0;
            }
        }
    }
}
