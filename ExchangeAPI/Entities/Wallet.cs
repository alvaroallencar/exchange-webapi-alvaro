namespace ExchangeAPI.Entities;

public class Wallet
{
    public int Id { get; set; }
    public required Currency Currency { get; set; }
    public decimal Balance { get; set; } = decimal.Zero;

    public decimal ExchangeRate
    {
        get
        {
            return Currency switch
            {
                Currency.Dollar => 0.93m,
                Currency.Euro => 1.08m,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}

public enum Currency
{
    Dollar,
    Euro
}