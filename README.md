# Student Loan Payoff Calculator

This project is a C# console app designed to help you figure out if it’s worth paying off your UK student loan (Plan 2) early. It takes your income, interest rate, loan balance, and time left on the loan to calculate potential savings in different repayment scenarios. 

## Features

- **Interest Rate Fetching**: Uses an API to fetch the current UK interest rate, adding 3% to match the Plan 2 loan rates.
- **Standard vs Early Repayment Comparison**: Calculates monthly payments and interest for:
  - Early payoff scenarios (after 1, 5, or 10 years)
  - Immediate payoff (saves on long-term interest)
- **Savings Display**: Shows the projected savings if paying off early, monthly repayment savings, and interest saved.

## Getting Started

### Prerequisites
- .NET 7.0 or higher (for `dotnet new gitignore` support).
- An API key from [api-ninjas.com](https://api-ninjas.com/) for interest rate data.

### Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/yourusername/student-loan-calculator.git
   cd student-loan-calculator
2. **Set the API Key**:
   - Set your API key as an environment variable so it's securely accessed:
     - **Windows**:
       ```bash
       setx API_NINJAS_KEY "YOUR_API_KEY"
       ```
     - **Mac/Linux**:
       ```bash
       export API_NINJAS_KEY="YOUR_API_KEY"
       ```

3. **Run the app**:
   ```bash
   dotnet run
