using Microsoft.AspNetCore.Http;
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
        public async Task<JsonResult> CallApi(string EndPoint,string ApiMethod,JsonElement body)
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
                return new JsonResult(new
                {
                    data = string.Empty,
                    message = string.Empty
                });
                
            }
            else if(ApiMethod == ApiMethods.Delete)
            {
                return await ApiClient.DeleteAsync(endPoint,token, "X-API-Key", key);
            }
            else {

                return new JsonResult(new
                {
                    data = string.Empty,
                    message = string.Empty
                });
            }
        }




        public async Task<string> CallApi_String(string EndPoint, string ApiMethod, JsonElement body)
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
                return await ApiClient.GetAsync_String(endPoint, token, "X-API-Key", key);
            }
            else if (ApiMethod == ApiMethods.Post)
            {
                return await ApiClient.PostAsync_String(endPoint, token, "X-API-Key", key, BodyStr);
            }
            else
            {
                return "";
            }
        }


    }
}
