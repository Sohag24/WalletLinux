namespace WalletApp.Model
{
    public class TransactionInfoDTO
    {
        public string UserId { get; set; }
        public decimal InAmount { get; set; }

        public decimal OutAmount { get; set; }
        public decimal FrozenAmount { get; set; }
    }
}
