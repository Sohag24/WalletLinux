using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.Json;
using WebApplication2.Helper;

namespace WebApplication2.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ValuesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet("SayHello")]
        public string SayHello()
        {
            string message = "Hello World!";
            return message;
        }

        [AllowAnonymous]
        [HttpGet("GetDate")]
        public string GetDate()
        {
            
            return GetUTCDateTime().ToString();
        }

        [AllowAnonymous]
        [HttpGet("GetDate2")]
        public string GetDate2()
        {

            return GetUTCDateTime2().ToString();
        }

        [HttpGet("GetVaults")]
        public async Task<string> GetVaults()
        {
            JsonElement EmptyJson = new JsonElement();
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(EndPoints.VaultAccounts, ApiMethods.Get, EmptyJson);
        }

        public static DateTime GetUTCDateTime()
        {
            var httpClient = new HttpClient();

            // Set the URL of the World Time API endpoint
            var apiUrl = "http://worldtimeapi.org/api/timezone/Asia/Dhaka";

            // Send an HTTP GET request to the API endpoint and get the response
            var response = httpClient.GetAsync(apiUrl).Result;

            // Read the response content as a string
            var responseContent = response.Content.ReadAsStringAsync().Result;

            var jsonObject = JObject.Parse(responseContent);

            // Get the value of the "name" variable as a string
            var utc_datetime = (DateTime)jsonObject["utc_datetime"];


            // Parse the response JSON to get the current UTC datetime
            //var dateTimeUtc = JsonConvert.DeserializeObject<string>(responseContent);

            return utc_datetime;

        }

        public static DateTime GetUTCDateTime2()
        {
            var httpClient = new HttpClient();

            // Set the URL of the World Time API endpoint
            var apiUrl = "https://worldtimeapi.org/api/timezone/Asia/Dhaka";

            // Send an HTTP GET request to the API endpoint and get the response
            var response = httpClient.GetAsync(apiUrl).Result;

            // Read the response content as a string
            var responseContent = response.Content.ReadAsStringAsync().Result;

            var jsonObject = JObject.Parse(responseContent);

            // Get the value of the "name" variable as a string
            var utc_datetime = (DateTime)jsonObject["utc_datetime"];


            // Parse the response JSON to get the current UTC datetime
            //var dateTimeUtc = JsonConvert.DeserializeObject<string>(responseContent);

            return utc_datetime;

        }
    }
}
