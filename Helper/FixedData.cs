namespace WalletApp.Helper
{
    public static class EndPoints
    {
        public const string VaultCreate = "/v1/vault/accounts";
        public const string SupportedAssets = "/v1/supported_assets";
        public const string VaultAccounts = "/v1/vault/accounts_paged";
        public const string Transactions = "/v1/transactions";
        public const string InternalWallets = "/v1/internal_wallets";
        public const string ExternalWallets = "/v1/external_wallets";
        public const string Contracts = "/v1/contracts";
        public const string Transaction = "/v1/transactions";

        // Coin Market
        public const string TokensPrice = "v1/cryptocurrency/listings/latest";
        public const string PriceView = "v2/cryptocurrency/quotes/historical";
        public const string AssetMarketCap = "v2/cryptocurrency/quotes/latest";
        

    }

    public static class ApiMethods
    {
        public const string Get = "Get";
        public const string Post = "Post";
        public const string Put = "Put";
        public const string Delete = "Delete";
    }
}
