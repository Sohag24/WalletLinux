namespace WalletApp.Model
{
    public class Tokens
    {
        public int id { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public int desimals { get; set; }
        public string address { get; set; }
        public string logoURI { get; set; }

    }

    public class Token
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public int Decimals { get; set; }
        public string Address { get; set; }
        public string LogoURI { get; set; }
        public List<string> Tags { get; set; }
    }

    public class TokenList
    {
        public Dictionary<string, Token> Tokens { get; set; }
    }
}
