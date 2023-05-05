namespace WalletApp.Model
{
    public class TransactionInfo
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
    }
}
