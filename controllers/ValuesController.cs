using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using WalletApp.Helper;
using WalletApp.Model;

namespace WebApplication2.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DBClass _dbContext;
        public ValuesController(IConfiguration configuration, DBClass dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
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


        // POST api/<WalletController>
        [HttpPost("CreateVault")]
        public async Task<string> CreateVault([FromBody] JsonElement body)
        {
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            var response = await FG.CallApi(EndPoints.VaultCreate, ApiMethods.Post, body);


            // Add Vault Info . . .
            try
            {
                dynamic responseData = JObject.Parse(response);
                int VaultId = responseData.id;

                string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
                dynamic data = JObject.Parse(BodyStr);

                var newVault = new VaultInfo() { VaultId = VaultId, Tag = data.tag, Category = data.category };
                var VaultRepository = new Repository<VaultInfo>(_dbContext);
                var savedUser = await VaultRepository.SaveAsync(newVault);
            }
            catch (Exception ex) { }

            return response.ToString();
        }

        [HttpGet("GetVaults")]
        public async Task<string> GetVaults()
        {
            JsonElement EmptyJson = new JsonElement();
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(EndPoints.VaultAccounts, ApiMethods.Get, EmptyJson);
        }

        [HttpPost("GetAssetPercentage")]
        public async Task<string> GetAssetPercentage([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);

            string input = data.vaultList;
            string[] vaults = input.Split(',');

            dynamic AssetList=null;
            List<dynamic> mergedList = new List<dynamic>();
            foreach (string vault in vaults)
            {
                int vaultId = int.Parse(vault);

                string endPoint = EndPoints.VaultCreate + "/" + vaultId;
                JsonElement EmptyJson = new JsonElement();

                FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
                var VaultList = await FG.CallApi(endPoint, ApiMethods.Get, EmptyJson);

                JObject json = JObject.Parse(VaultList);

                AssetList = json["assets"];

                //var objects = JsonConvert.DeserializeObject<dynamic[]>(AssetList);
                mergedList.AddRange(AssetList);
            }


            

            // Calculate total balance
            decimal totalBalance = 0;
            foreach (var assets in mergedList)
            {   
                
                decimal balance = decimal.Parse(assets.balance.ToString());
                totalBalance += balance;
            }

            // Calculate percentage of total balance for each asset
            Dictionary<string, decimal> assetPercentages = new Dictionary<string, decimal>();
            foreach (var asset in mergedList)
            {
                string id = asset.id.ToString();
                decimal balance = decimal.Parse(asset.balance.ToString());
                decimal percentage = 0;
                if (totalBalance != 0)
                {
                     percentage = (balance / totalBalance) * 100;
                }
                if (assetPercentages.ContainsKey(id))
                {
                    // Update the existing value
                    assetPercentages[id] = assetPercentages[id]+ percentage;
                }
                else
                {
                    // Add a new key-value pair to the dictionary
                    assetPercentages.Add(id, percentage);
                }
            }

            // Create JSON object with total balance and asset percentages
            dynamic resultObject = new ExpandoObject();
            resultObject.totalBalance = totalBalance;
            resultObject.assetPercentages = assetPercentages;

            // Convert JSON object to string
            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(resultObject);

            return jsonString;

        }

         // GET api/<WalletController>
        [HttpPost("GetVaultsByVaultId")]
        public async Task<string> GetVaultsByVaultId([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.VaultCreate + "/" + data.vaultId;
            string? eosAccountName = data.eosAccountName;

            JsonElement EmptyJson = new JsonElement();

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Get, EmptyJson);
        }

        [HttpGet("GetTags")]
        public List<Tag> GetTags()
        {
            var Repository = new Repository<Tag>(_dbContext);
            var Info = Repository.GetAllAsync().Result;
            var Data = Info.ToList();

            return Data;
        }

        [HttpGet("GetCategories")]
        public List<Category> GetCategories()
        {
            var Repository = new Repository<Category>(_dbContext);
            var Info = Repository.GetAllAsync().Result;
            var Data = Info.ToList();

            return Data;
        }

        // GET api/<WalletController>
        [HttpPost("GetVaultInfo")]
        public List<VaultInfoDTO> GetVaultInfo([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            int VaultId = data.vaultId ?? 0;
            int TagId = data.tagId ?? 0;
            int CategoryId = data.categoryId ?? 0;

            var vaultRepository = new Repository<VaultInfo>(_dbContext);
            var VaultInfo = vaultRepository.GetAllAsync().Result;
            var Vault = VaultInfo.Where(a => (a.VaultId == VaultId || VaultId == 0)
            && (a.Category == CategoryId || CategoryId==0) && (a.Tag==TagId || TagId==0) ).ToList();

            var categoryRepository = new Repository<Category>(_dbContext);
            var CategoryInfo = categoryRepository.GetAllAsync().Result;

            var tagRepository = new Repository<Tag>(_dbContext);
            var TagInfo = tagRepository.GetAllAsync().Result;

            var result = from v in Vault
                         join c in CategoryInfo on v.Category equals c.Id
                         join t in TagInfo on v.Tag equals t.Id
                         select new VaultInfoDTO { vaultId=v.VaultId, tag=t.Name, category=c.Name };


            if (result == null)
            {
                return new List<VaultInfoDTO>();
            }
            else
            {
                return result.ToList();
            }

            
        }

        // Get api/<WalletController>
        [HttpGet("GetSupportedAssets")]
        public async Task<string> GetSupportedAssets()
        {
            JsonElement EmptyJson = new JsonElement();
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(EndPoints.SupportedAssets, ApiMethods.Get, EmptyJson);
        }

        // POST api/<WalletController>
        [HttpPost("AddAssets")]
        public async Task<string> AddAssets([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.VaultCreate + "/" + data.vaultId + "/" + data.assetId;
            string? eosAccountName = data.eosAccountName;

            JsonElement newBody = new JsonElement();
            using (MemoryStream stream = new MemoryStream())
            {
                using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();

                    writer.WriteString("eosAccountName", eosAccountName);

                    writer.WriteEndObject();
                }

                byte[] json = stream.ToArray();
                JsonDocument document = JsonDocument.Parse(json);
                newBody = document.RootElement;
            }


            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Post, newBody);
        }


        [HttpPost("HideVault")]
        public async Task<string> HideVault([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.VaultCreate + "/" + data.vaultAccountId + "/hide";

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Post, body);
        }

        [HttpPost("UnhideVault")]
        public async Task<string> UnhideVault([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.VaultCreate + "/" + data.vaultAccountId + "/unhide";

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Post, body);
        }

        [HttpPost("SetAutoFuel")]
        public async Task<string> SetAutoFuel([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.VaultCreate + "/" + data.vaultAccountId + "/set_auto_fuel";

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Post, body);
        }

        [HttpPost("Activate")]
        public async Task<string> Activate([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.VaultCreate + "/" + data.vaultAccountId + "/" + data.assetId + "/activate";

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Post, body);
        }

        [HttpGet("GetTransactions")]
        public async Task<string> GetTransactions()
        {
            var QueryString = HttpContext.Request.QueryString;
            string endPoint = EndPoints.Transactions + QueryString;
            JsonElement EmptyJson = new JsonElement();
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Get, EmptyJson);
        }

        [HttpPost("GetTransactionAmount")]
        public TransactionInfoDTO GetTransactionAmount([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);

            var Repository = new Repository<TransactionInfo>(_dbContext);
            var Info = Repository.GetAllAsync().Result;
            var Data = Info.Where(d=>d.UserId== data.UserId).ToList();

            decimal InAmount = 0;
            decimal OutAmount = 0;
            if (Data==null)
            {
                return new TransactionInfoDTO();
            }
            else
            {
                InAmount=Data.Where(a=>a.TransactionType=="IN").Sum(a=>a.Amount);
                OutAmount = Data.Where(a => a.TransactionType == "OUT").Sum(a => a.Amount);
                return new TransactionInfoDTO() { UserId=data.UserId,InAmount= InAmount ,OutAmount= OutAmount ,FrozenAmount=0};
            }

            //return Data;
        }

        [HttpPost("TransactionEstimateFee")]
        public async Task<string> TransactionEstimateFee([FromBody] JsonElement body)
        {
            
            string endPoint = EndPoints.Transactions + "/estimate_fee";
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Post, body);
        }

        // GET api/<WalletController>
        [HttpPost("GetTransactionById")]
        public async Task<string> GetTransactionById([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.Transactions + "/" + data.txId;

            JsonElement EmptyJson = new JsonElement();

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Get, EmptyJson);
        }

        [HttpGet("GetInternalWallets")]
        public async Task<string> GetInternalWallets()
        {
            JsonElement EmptyJson = new JsonElement();
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(EndPoints.InternalWallets, ApiMethods.Get, EmptyJson);
        }

        [HttpGet("GetExternalWallets")]
        public async Task<string> GetExternalWallets()
        {
            JsonElement EmptyJson = new JsonElement();
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(EndPoints.ExternalWallets, ApiMethods.Get, EmptyJson);
        }

        [HttpGet("GetContracts")]
        public async Task<string> GetContracts()
        {
            JsonElement EmptyJson = new JsonElement();
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(EndPoints.Contracts, ApiMethods.Get, EmptyJson);
        }

        // POST api/<WalletController>
        [HttpPost("CreateInternalWallet")]
        public async Task<string> CreateInternalWallet([FromBody] JsonElement body)
        {
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(EndPoints.InternalWallets, ApiMethods.Post, body);
        }

        // Delete api/<WalletController>
        [HttpPost("DeleteInternalWallet")]
        public async Task<string> DeleteInternalWallet([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.InternalWallets + "/" + data.WalletId;

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Delete, body);
        }

        // Delete api/<WalletController>
        [HttpPost("DeleteAssetInternalWallet")]
        public async Task<string> DeleteAssetInternalWallet([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.InternalWallets + "/" + data.WalletId+"/"+data.AssetId;

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Delete, body);
        }

        // POST api/<WalletController>
        [HttpPost("CreateExternalWallet")]
        public async Task<string> CreateExternalWallet([FromBody] JsonElement body)
        {
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(EndPoints.ExternalWallets, ApiMethods.Post, body);
        }

        // Delete api/<WalletController>
        [HttpPost("DeleteExternalWallet")]
        public async Task<string> DeleteExternalWallet([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.ExternalWallets + "/" + data.WalletId;

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Delete, body);
        }

        // Delete api/<WalletController>
        [HttpPost("DeleteAssetExternalWallet")]
        public async Task<string> DeleteAssetExternalWallet([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.ExternalWallets + "/" + data.WalletId + "/" + data.AssetId;

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Delete, body);
        }

        // POST api/<WalletController>
        [HttpPost("CreateContracts")]
        public async Task<string> CreateContracts([FromBody] JsonElement body)
        {
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(EndPoints.Contracts, ApiMethods.Post, body);
        }

        // Delete api/<WalletController>
        [HttpPost("DeleteContracts")]
        public async Task<string> DeleteContracts([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.Contracts + "/" + data.contractId;

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Delete, body);
        }

        // Delete api/<WalletController>
        [HttpPost("DeleteAssetContracts")]
        public async Task<string> DeleteAssetContracts([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.Contracts + "/" + data.contractId + "/" + data.AssetId;

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Delete, body);
        }

        // POST api/<WalletController>
        [HttpPost("AddAssetsInternalWallet")]
        public async Task<string> AddAssetsInternalWallet([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.InternalWallets + "/" + data.walletId + "/" + data.assetId;
            string? address = data.address;

            JsonElement newBody = new JsonElement();
            using (MemoryStream stream = new MemoryStream())
            {
                using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();

                    writer.WriteString("address", address);

                    writer.WriteEndObject();
                }

                byte[] json = stream.ToArray();
                JsonDocument document = JsonDocument.Parse(json);
                newBody = document.RootElement;
            }


            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Post, newBody);
        }


        // POST api/<WalletController>
        [HttpPost("AddAssetsExternalWallet")]
        public async Task<string> AddAssetsExternalWallet([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.ExternalWallets + "/" + data.walletId + "/" + data.assetId;
            string? address = data.address;

            JsonElement newBody = new JsonElement();
            using (MemoryStream stream = new MemoryStream())
            {
                using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();

                    writer.WriteString("address", address);

                    writer.WriteEndObject();
                }

                byte[] json = stream.ToArray();
                JsonDocument document = JsonDocument.Parse(json);
                newBody = document.RootElement;
            }


            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Post, newBody);
        }

        // POST api/<WalletController>
        [HttpPost("AddAssetsContract")]
        public async Task<string> AddAssetsContract([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.Contracts + "/" + data.walletId + "/" + data.assetId;
            string? address = data.address;

            JsonElement newBody = new JsonElement();
            using (MemoryStream stream = new MemoryStream())
            {
                using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();

                    writer.WriteString("address", address);

                    writer.WriteEndObject();
                }

                byte[] json = stream.ToArray();
                JsonDocument document = JsonDocument.Parse(json);
                newBody = document.RootElement;
            }


            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Post, newBody);
        }


        // POST api/<WalletController>
        [HttpPost("GetAssetDepositAddress")]
        public async Task<string> GetAssetDepositAddress([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.VaultCreate + "/" + data.vaultId + "/" + data.assetId+ "/addresses";
            string? eosAccountName = data.eosAccountName;

            JsonElement EmptyJson = new JsonElement();

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Get, EmptyJson);
        }


        // POST api/<WalletController>
        [HttpPost("transactions")]
        public async Task<string> transactions([FromBody] JsonElement body)
        {
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);

            // Add Transaction Info . . .
            try
            {
                string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
                dynamic data = JObject.Parse(BodyStr);

                var newTransaction = new TransactionInfo() { UserId = data.UserId, TransactionType = "OUT", Amount = data.amount };
                var txRepository = new Repository<TransactionInfo>(_dbContext);
                var savedUser = await txRepository.SaveAsync(newTransaction);

                var newTransactionTo = new TransactionInfo() { UserId = data.UserIdTo, TransactionType = "IN", Amount = data.amount };
                var txRepositoryTo = new Repository<TransactionInfo>(_dbContext);
                var savedUserTo = await txRepositoryTo.SaveAsync(newTransactionTo);
            }
            catch (Exception ex) { }

            return await FG.CallApi(EndPoints.Transaction, ApiMethods.Post, body);

        }



        [HttpPost("FreezeTransaction")]
        public async Task<string> FreezeTransaction([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.Transactions + "/" + data.txId + "/freeze";

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Post, body);
        }

        [HttpPost("UnfreezeTransaction")]
        public async Task<string> UnfreezeTransaction([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.Transaction + "/" + data.txId + "/unfreeze";

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Post, body);
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


        // Coin Market API
        [HttpGet("GetTokensPrice")]
        public async Task<string> GetTokensPrice()
        {
           CoinMarket_GateWay CG=new CoinMarket_GateWay(_configuration);
            return await CG.CallApi(EndPoints.TokensPrice, ApiMethods.Get);
        }

        [HttpGet("GetPriceView")]
        public async Task<string> GetPriceView()
        {
            var QueryString = HttpContext.Request.QueryString;
            string endPoint = EndPoints.PriceView + QueryString;
        
            CoinMarket_GateWay CG = new CoinMarket_GateWay(_configuration);
            return await CG.CallApi(endPoint, ApiMethods.Get);
        }

        [HttpGet("GetAssetMarketCap")]
        public async Task<string> GetAssetMarketCap()
        {
            var QueryString = HttpContext.Request.QueryString;
            string endPoint = EndPoints.AssetMarketCap + QueryString;

            CoinMarket_GateWay CG = new CoinMarket_GateWay(_configuration);
            return await CG.CallApi(endPoint, ApiMethods.Get);
        }
    }
}
