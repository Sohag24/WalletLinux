using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WalletApp.Helper;
using WalletApp.Model;


namespace WalletApp.Helper
{
    public  class FireBlocks_GateWay
    {
        private readonly IConfiguration _configuration;

        public FireBlocks_GateWay(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> CallApi(string EndPoint,string ApiMethod,JsonElement body)
        {
            Configuration con = new Configuration(_configuration);
            ConfigurationDTO configuration = con.getConfiguration();

            string endPoint = EndPoint;
            string BodyStr = "";
            if (ApiMethod == ApiMethods.Post)
            {
                BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            }
            string key = configuration.Key;
            string baseurl = configuration.FireBlocks_BaseURL;
            var token = JwtGenerator.GenerateJwtToken(endPoint, ApiMethod, BodyStr, key);

            RestApiClient ApiClient = new RestApiClient(baseurl);
            if (ApiMethod == ApiMethods.Get)
            {
                return await ApiClient.GetAsync(endPoint, token, "X-API-Key", key);
            }
            else if (ApiMethod == ApiMethods.Post)
            {
                return await ApiClient.PostAsync(endPoint, token, "X-API-Key", key, BodyStr);
            }
            else if(ApiMethod == ApiMethods.Put)
            {
                return await Task.FromResult<string>("Not Implemented");
            }
            else if(ApiMethod == ApiMethods.Delete)
            {
                return await ApiClient.DeleteAsync(endPoint,token, "X-API-Key", key);
            }
            else {
                return await Task.FromResult<string>("Not Implemented");
            }
        }
    }
}
