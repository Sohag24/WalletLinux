using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WalletApp.Helper;
using WalletApp.Model;


namespace WalletApp.Helper
{
    public  class CoinMarket_GateWay
    {
        private readonly IConfiguration _configuration;

        public CoinMarket_GateWay(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> CallApi(string EndPoint,string ApiMethod)
        {
            Configuration con = new Configuration(_configuration);
            ConfigurationDTO configuration = con.getConfiguration();

            string endPoint = EndPoint;
           
            string key = configuration.CoinMarketAPIKey;
            string baseurl = configuration.CoinMarket_BaseURL;

            RestApiClient ApiClient = new RestApiClient(baseurl);
            if (ApiMethod == ApiMethods.Get)
            {
                return await ApiClient.GetAsync(endPoint, "", "X-CMC_PRO_API_KEY", key);
            }
            else if (ApiMethod == ApiMethods.Post)
            {
                return await ApiClient.PostAsync(endPoint, "", "X-CMC_PRO_API_KEY", key, "");
            }
            else if(ApiMethod == ApiMethods.Put)
            {
                return await Task.FromResult<string>("Not Implemented");
            }
            else if(ApiMethod == ApiMethods.Delete)
            {
                return await ApiClient.DeleteAsync(endPoint,"", "X-API-Key", key);
            }
            else {
                return await Task.FromResult<string>("Not Implemented");
            }
        }
    }
}
