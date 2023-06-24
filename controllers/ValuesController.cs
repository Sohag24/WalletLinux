using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
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
        [HttpPost("CreateAccount")]
        public async Task<string> CreateAccount([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            var Accounttype = data.AccountType;
            var userId = data.userId.ToString();

            // Add Account Info . . .
            try
            {
                var newData = new AccountInfo() { AccountType = Accounttype, UserId = userId };
                var Repository = new Repository<AccountInfo>(_dbContext);
                var savedUser = await Repository.SaveAsync(newData);
                Response.StatusCode = 200; // Set the HTTP status code to 200
                return "Account Creation Successfull!";
            }
            catch (Exception ex) {
                Response.StatusCode = 500; // Set the HTTP status code to 500
                return "Account Creation Failed! Exception: "+ex.Message;
            }

        }

        // POST api/<WalletController>
        [HttpPost("CreateCategory")]
        public async Task<string> CreateCategory([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            var Name = data.Name.ToString(); 
            var userId = data.userId.ToString();

            // Add Info . . .
            try
            {
                var newData = new Category() { Name = Name, UserId = userId };
                var Repository = new Repository<Category>(_dbContext);
                var savedUser = await Repository.SaveAsync(newData);
                Response.StatusCode = 200; // Set the HTTP status code to 200
                return "Category Creation Successfull!";
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Set the HTTP status code to 500
                return "Category Creation Failed! Exception: " + ex.Message;
            }

        }

        // POST api/<WalletController>
        [HttpPost("CreateTag")]
        public async Task<string> CreateTag([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            var Name = data.Name.ToString();
            var userId = data.userId.ToString();

            // Add Info . . .
            try
            {
                var newData = new Tag() { Name = Name, UserId = userId };
                var Repository = new Repository<Tag>(_dbContext);
                var savedUser = await Repository.SaveAsync(newData);
                Response.StatusCode = 200; // Set the HTTP status code to 200
                return "Tag Creation Successfull!";
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Set the HTTP status code to 500
                return "Tag Creation Failed! Exception: " + ex.Message;
            }

        }


        // POST api/<WalletController>
        [HttpPost("CreateVault")]
        public async Task<string> CreateVault([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            var SubscriptionLevel = data.SubscriptionLevel;
            var userId= data.userId.ToString();

            var vRepository = new Repository<VaultInfo>(_dbContext);
            var VaultInfo = vRepository.GetAllAsync().Result;
            int VautlCount = VaultInfo.Where(a => a.UserId == userId).Count();

            var aRepository = new Repository<AccountInfo>(_dbContext);
            var AccountInfo = aRepository.GetAllAsync().Result;
            var Account = AccountInfo.Where(a => a.UserId == userId).FirstOrDefault();

            string AccountType = "Personal";
            if(Account!= null)
            {
                AccountType = Account.AccountType;
            }
            int goldAmount = 4;
            if(AccountType=="Personal")
            {
                goldAmount = 3;
            }

            if (SubscriptionLevel == "FREE" && VautlCount >= 1)
            {
                Response.StatusCode = 403; // Set the HTTP status code to 403
                return "Your account only supports 1 vaults.  If you would like to add further vault accounts please ";
            }
            else if(SubscriptionLevel=="SILVER" && VautlCount>=2)
            {
                Response.StatusCode = 403; // Set the HTTP status code to 403
                return "Your account only supports 2 vaults.  If you would like to add further vault accounts please ";
            }
            else if (SubscriptionLevel == "GOLD" && VautlCount >= goldAmount)
            {
                Response.StatusCode = 403; // Set the HTTP status code to 403
                return "Your account only supports "+ goldAmount + " vaults.  If you would like to add further vault accounts please ";
            }
            else if (SubscriptionLevel == "PLATINUM")
            {

            }
            else{}

            // Create Vault in Fireblocks
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            var response = await FG.CallApi(EndPoints.VaultCreate, ApiMethods.Post, body);


            // Add Vault Info . . .
            try
            {
                dynamic responseData = JObject.Parse(response);
                int VaultId = responseData.id;

                var newVault = new VaultInfo() { VaultId = VaultId,UserId= data.userId, Tag = 0, Category =0  };
                var VaultRepository = new Repository<VaultInfo>(_dbContext);
                var savedVault = await VaultRepository.SaveAsync(newVault);

                //Save Tag

                string tags = data.tag;
                if(tags != "") {
                    string[] tagsArray = tags.Split(',');
                    foreach( string tag in tagsArray )
                    {
                        var newData = new VaultWiseTags() { VaultId = VaultId, TagId= Convert.ToInt32(tag) };
                        var VWRepository = new Repository<VaultWiseTags>(_dbContext);
                        var savedt = await VWRepository.SaveAsync(newData);
                    }
                }


                //Save Category

                string categories = data.category;
                if (categories != "")
                {
                    string[] categoriesArray = categories.Split(',');
                    foreach (string category in categoriesArray)
                    {
                        var newData = new VaultWiseCategories() { VaultId = VaultId, CategoryId = Convert.ToInt32(category) };
                        var VWRepository = new Repository<VaultWiseCategories>(_dbContext);
                        var savedt = await VWRepository.SaveAsync(newData);
                    }
                }


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

            JsonElement EmptyJsons = new JsonElement();
            FireBlocks_GateWay FGs = new FireBlocks_GateWay(_configuration);
            var jsons = await FGs.CallApi(EndPoints.VaultAccounts, ApiMethods.Get, EmptyJsons);

            // Deserialize the JSON string into RootObject
            Vaults rootObject = JsonConvert.DeserializeObject<Vaults>(jsons);

            // Create a new RootObject with filtered accounts
            Vaults filteredRootObject = new Vaults
            {
                Accounts = rootObject.Accounts
                    .Where(account => vaults.Contains(account.Id))
                    .ToArray()
            };

            // Serialize the filteredRootObject back to JSON string
            //string filteredJson = JsonConvert.SerializeObject(filteredRootObject, Formatting.Indented);

            // Calculate total balance
            decimal totalBalance = 0;
            
            foreach (var accounts in filteredRootObject.Accounts)
            {
                var i = 0;
                foreach (var asset in accounts.Assets)
                {
                    decimal balance = decimal.Parse(asset.Balance.ToString());
                    totalBalance += balance;
                    i++;
                }
            }

            
            // Calculate percentage of total balance for each asset
            Dictionary<string, decimal> assetPercentages = new Dictionary<string, decimal>();
            foreach (var accounts in filteredRootObject.Accounts)
            {
                foreach (var asset in accounts.Assets)
                {
                    string id = asset.Id.ToString();
                    decimal balance = decimal.Parse(asset.Balance.ToString());
                    decimal percentage = 0;
                    if (totalBalance != 0)
                    {
                        percentage = (balance / totalBalance) * 100;
                    }

                    if (assetPercentages.ContainsKey(id))
                    {
                        // Update the existing value																									
                        assetPercentages[id] = assetPercentages[id] + percentage;
                    }
                    else
                    {
                        // Add a new key-value pair to the dictionary																									
                        assetPercentages.Add(id, percentage);
                    }
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

        [HttpPost("GetTags")]
        public List<Tag> GetTags([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);

            string userId = data.userId.ToString();

            var Repository = new Repository<Tag>(_dbContext);
            var Info = Repository.GetAllAsync().Result;
            var Data = Info.Where(d=>d.UserId == userId).ToList();

            return Data;
        }

        [HttpPost("GetCategories")]
        public List<Category> GetCategories([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);

            string userId = data.userId.ToString();

            var Repository = new Repository<Category>(_dbContext);
            var Info = Repository.GetAllAsync().Result;
            var Data = Info.Where(d => d.UserId == userId).ToList();

            return Data;
        }

        // GET api/<WalletController>
        [HttpPost("GetVaultInfo")]
        public List<VaultInfoDTO> GetVaultInfo([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
           
            string UserId = data.userId ?? 0;

            var vaultRepository = new Repository<VaultInfo>(_dbContext);
            var VaultInfo = vaultRepository.GetAllAsync().Result;
            var Vault = VaultInfo.Where(a => (a.UserId == UserId || UserId == "0") ).ToList();

            var categoryRepository = new Repository<Category>(_dbContext);
            var CategoryInfo = categoryRepository.GetAllAsync().Result;

            var tagRepository = new Repository<Tag>(_dbContext);
            var TagInfo = tagRepository.GetAllAsync().Result;

            var vaultWiseCategoryRepository = new Repository<VaultWiseCategories>(_dbContext);
            var vaultWiseCategory = vaultWiseCategoryRepository.GetAllAsync().Result;

            var vaultWiseTagRepository = new Repository<VaultWiseTags>(_dbContext);
            var vaultWiseTag = vaultWiseTagRepository.GetAllAsync().Result;

            var result = from v in Vault
                         join vc in vaultWiseCategory on v.VaultId equals vc.VaultId
                         join vt in vaultWiseTag on v.VaultId equals vt.VaultId
                         join c in CategoryInfo on vc.CategoryId equals c.Id
                         join t in TagInfo on vt.TagId equals t.Id                       
                         select new VaultInfoDTO { vaultId=v.VaultId,tagId=t.Id, tag=t.Name,categoryId=c.Id, category=c.Name };


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

        [HttpPost("GetActiveTransactions")]
        public async Task<string> GetActiveTransactions([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);

            var QueryString = HttpContext.Request.QueryString;
            string endPoint = EndPoints.Transactions + QueryString;
            JsonElement EmptyJson = new JsonElement();
            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            var transactionResp= await FG.CallApi(endPoint, ApiMethods.Get, EmptyJson);

            // Get Active transaction ID from DB
            var transactionRepository = new Repository<TransactionInfo>(_dbContext);
            var TransactionInfo = transactionRepository.GetAllAsync().Result;
            var userId = data.userId.ToString();
            var ActiveTransactions = TransactionInfo.Where(a => a.UserId == userId && a.IsActive == true).ToList();

            // If there are active transactions
            if (ActiveTransactions != null)
            {

                // Deserialize the JSON object
                List<dynamic> rootObject = JsonConvert.DeserializeObject<List<dynamic>>(transactionResp);

                // Join the JSON object with the active transactions
                //List<dynamic> filteredRootObject = rootObject.Join(
                //    ActiveTransactions,
                //    transaction => transaction.id,
                //    filter => filter.txId,
                //    (transaction, filter) => transaction
                //).ToList();

                List<dynamic> filteredRootObject = rootObject.Where(item => ActiveTransactions.Any(id => item.id == id.txId)).ToList();
                List<dynamic> TransferList = rootObject.Where(item => ActiveTransactions.Any(id => item.id == id.txId && id.TransactionType=="OUT" && id.IsMailSent==false)).ToList();

                if (TransferList != null)
                {
                    SendMail(TransferList, userId);
                }

                // Serialize the filtered JSON object
                string filteredJson = JsonConvert.SerializeObject(filteredRootObject, Formatting.Indented);

                // Return the filtered JSON object
                return filteredJson;

            }
            else
            {
                return transactionResp;
            }

        }

        public  async Task<string> SendMail(List<dynamic> TransferList,string userId)
        {
            try
            {
                foreach (var item in TransferList)
                {
                    string guid = userId;
                    string status = item.status;
                    string txId = item.id;

                    if (status == "COMPLETED" || status == "CANCELLED" || status == "REJECTED" || status == "BLOCKED" || status == "FAILED")
                    {
                        JsonElement newBody = new JsonElement();
                        using (MemoryStream stream = new MemoryStream())
                        {
                            using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
                            {
                                writer.WriteStartObject();

                                writer.WriteString("guid", guid);
                                writer.WriteString("status", status);

                                writer.WriteEndObject();
                            }

                            byte[] json = stream.ToArray();
                            JsonDocument document = JsonDocument.Parse(json);
                            newBody = document.RootElement;
                        }

                        // Call API . . . . .
                        var httpClient = new HttpClient();
                        // Set the URL of the World Time API endpoint
                        var apiUrl = "https://clearchainx-dev-web-app-api.azurewebsites.net/wallet/notification?guid=" + userId + "&status=" + status;
                        var jsonContent = new StringContent("", Encoding.UTF8, "application/json");
                        // Send an HTTP GET request to the API endpoint and get the response
                        var response = httpClient.PostAsync(apiUrl, jsonContent).Result;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var upRepository = new Repository<TransactionInfo>(_dbContext);
                            var sql = "update TransactionInfo set IsMailSent=1 where UserId='" + userId + "' and IsMailSent=0 and txId='" + txId + "'";
                            var a = await upRepository.ExecuteSQL(sql);
                        }

                    }

                }

                return "Ok";

            }
            catch (Exception ex) { return "Not Ok"; }
        }

      

        [HttpPost("GetTransactionAmount")]
        public TransactionInfoDTO GetTransactionAmount([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);

            var Repository = new Repository<TransactionInfo>(_dbContext);
            var Info = Repository.GetAllAsync().Result;
            var userId= data.UserId.ToString();
            var Data = Info.Where(d=>d.UserId== userId).ToList();

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
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);

            //TransactionFeeTransfer(data);

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            var TransactionResp= await FG.CallApi(EndPoints.Transaction, ApiMethods.Post, body);

            // Add Transaction Info . . .
            try
            {
                JObject transaction = JObject.Parse(TransactionResp);
                var txId = transaction["id"].ToString();

                var newTransaction = new TransactionInfo() { UserId = data.UserId, TransactionType = "OUT", Amount = data.amount,txId= txId,IsActive=true };
                var txRepository = new Repository<TransactionInfo>(_dbContext);
                var savedUser = await txRepository.SaveAsync(newTransaction);

                var newTransactionTo = new TransactionInfo() { UserId = data.UserIdTo, TransactionType = "IN", Amount = data.amount,txId="0", IsActive = false };
                var txRepositoryTo = new Repository<TransactionInfo>(_dbContext);
                var savedUserTo = await txRepositoryTo.SaveAsync(newTransactionTo);
            }
            catch (Exception ex) { }

            // Transaction Fee . . .
            try
            {
                TransactionFeeTransfer(data);
            }
            catch (Exception ex) { }

            return TransactionResp;

        }


        public async Task<string> TransactionFeeTransfer(dynamic data)
        {
            try
            {

                decimal Amount = data.amount;
                string assetId= data.assetId;
                decimal Rate = GetRate(assetId);
                decimal DollarAmount = Rate * Amount;
                decimal Fee = (1 / 100) * DollarAmount;
                if (DollarAmount < 500)
                {
                    Fee = Fee + (decimal)0.5;
                }

                decimal ETHFee = (1 / Rate) * Fee;

                // Transfer .......
                data.destination.type = "EXTERNAL_WALLET";
                data.destination.id = "18dbc250-5997-48b3-8987-95ff851b835a";
                //data.destination.oneTimeAddress.address = "0xe32CA17AAA9391eb0c22513b9A2509Cf6562A629";
                data.amount = ETHFee;

                string reversedJson = data.ToString();
                JsonDocument reversedBody = JsonDocument.Parse(reversedJson);
                JsonElement reversedElement = reversedBody.RootElement;

                FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
                var TransactionResp = await FG.CallApi(EndPoints.Transaction, ApiMethods.Post, reversedElement);

                return TransactionResp;

            } catch (Exception ex) { return ""; }  
        }

        // Get Convertion Rate

        public decimal GetRate(string assetId)
        {
            var fromToken = "0xeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
            var amounts = "1000000000000000000"; 

            int index = assetId.IndexOf('_');

            string RealAssetId= assetId;
            if (index != -1)
            {
                 RealAssetId = assetId.Substring(0, index);
                
            }

            var httpClient = new HttpClient();
            // Set the URL of the World Time API endpoint
            var apiUrl = "https://31.220.108.116:3002/tokens";
            // Send an HTTP GET request to the API endpoint and get the response
            var apiResponse = httpClient.GetAsync(apiUrl).Result;
            string responseContent = apiResponse.Content.ReadAsStringAsync().Result;

            TokenList tokenList = JsonConvert.DeserializeObject<TokenList>(responseContent);
            
            Token matchingToken = tokenList.Tokens.Values.FirstOrDefault(token => token.Symbol == RealAssetId);
            if (matchingToken != null)
            {
                string address = matchingToken.Address;
                int decimals = matchingToken.Decimals;
                fromToken = address;
                amounts = "1" + new string('0', decimals);

            }

            // Your JSON object
            var jsonObject = new
            {
                networkid = "1",
                excnetworkid = "1",
                fromtokenaddress = fromToken,
                totokenaddress = "0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48",
                amount = amounts
            };

            // Convert the object to a JSON string
            string body = JsonConvert.SerializeObject(jsonObject);

           
            // Set the URL of the World Time API endpoint
            apiUrl = "https://31.220.108.116:3002/getquote";
            var jsonContent = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
            // Send an HTTP GET request to the API endpoint and get the response
            var response = httpClient.PostAsync(apiUrl, jsonContent).Result;
            // Read the response content as a string
            responseContent = response.Content.ReadAsStringAsync().Result;
            var obj = JObject.Parse(responseContent);
            decimal amt = Convert.ToDecimal(obj["quote"]["toTokenAmount"]);
            return amt / 1000000;
        }

        [HttpPost("HideTransaction")]
        public async Task<bool> HideTransaction([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);

            var txId=data.txId;
            var userId = data.userId;

            var upRepository = new Repository<TransactionInfo>(_dbContext);

            var sql = "update TransactionInfo set IsActive=0 where UserId='" + userId + "' and IsActive=1 and txId='"+ txId+"'";
            if (txId == "0")
            {
                sql = "update TransactionInfo set IsActive=0 where UserId='" + userId + "' and IsActive=1";
            }
            return await upRepository.ExecuteSQL(sql);
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

        [HttpPost("SetKytForVault")]
        public async Task<string> SetKytForVault([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.VaultCreate + "/" + data.vaultAccountId + "/set_customer_ref_id";

            FireBlocks_GateWay FG = new FireBlocks_GateWay(_configuration);
            return await FG.CallApi(endPoint, ApiMethods.Post, body);
        }

        [HttpPost("SetKytForInternalWallet")]
        public async Task<string> SetKytForInternalWallet([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);
            string endPoint = EndPoints.InternalWallets + "/" + data.walletId + "/set_customer_ref_id";

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


        // Plaid .........

        [HttpPost("GetPlaidToken")]
        public string GetPlaidToken([FromBody] JsonElement body)
        {
            var httpClient = new HttpClient();
            // Set the URL of the World Time API endpoint
            var apiUrl = "https://sandbox.plaid.com/link/token/create";
            var jsonContent = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
            // Send an HTTP GET request to the API endpoint and get the response
            var response = httpClient.PostAsync(apiUrl, jsonContent).Result;
            // Read the response content as a string
            var responseContent = response.Content.ReadAsStringAsync().Result;

            return responseContent;
        }


        [HttpPost("UpdatePlaidStatus")]
        public string UpdatePlaidStatus([FromBody] JsonElement body)
        {
            var httpClient = new HttpClient();

            // Set the URL of the World Time API endpoint
            //var apiUrl = "https://sandbox.plaid.com/identity_verification/list";
            var apiUrl = "https://development.plaid.com/identity_verification/list";
        
            var jsonContent = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
            // Send an HTTP GET request to the API endpoint and get the response
            var response = httpClient.PostAsync(apiUrl, jsonContent).Result;
            // Read the response content as a string
            dynamic responseContent = response.Content.ReadAsStringAsync().Result;
            JsonDocument jsonDocument = JsonDocument.Parse(responseContent);        
           // Extracting statuses
            string kycStatus = jsonDocument.RootElement
                .GetProperty("identity_verifications")[0]
                .GetProperty("status")
                .GetString();

            if(kycStatus=="success")
            {
                string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
                dynamic data = JObject.Parse(BodyStr);
                string guid = data.client_user_id;
                // Set the URL of the World Time API endpoint
                apiUrl = "https://clearchainx-dev-web-app-api.azurewebsites.net/wallet/idv?guid="+ guid + "&status=active";
                // Send an HTTP GET request to the API endpoint and get the response
                var apiResponse = httpClient.PostAsync(apiUrl, jsonContent).Result;
                Response.StatusCode = 200;
                return apiResponse.ToString();
            }
            else
            {
                Response.StatusCode= 500;
                return response.ToString();
            }       

        }


        [HttpPost("RestCall")]
        public string RestCall([FromBody] JsonElement body)
        {
            string BodyStr = System.Text.Json.JsonSerializer.Serialize(body);
            dynamic data = JObject.Parse(BodyStr);

            var httpClient = new HttpClient();
            // Set the URL of the World Time API endpoint
            var apiUrl = data.URL.ToString();
            var apiMethod = data.Method;

            var jsonContent = new StringContent(body.ToString(), Encoding.UTF8, "application/json");

            // Send an HTTP GET request to the API endpoint and get the response
            if (apiMethod == "POST")
            {
                var response = httpClient.PostAsync(apiUrl, jsonContent).Result;
                // Read the response content as a string
                var responseContent = response.Content.ReadAsStringAsync().Result;
                return responseContent;
            }
            else
            {
                var response = httpClient.GetAsync(apiUrl).Result;
                // Read the response content as a string
                var responseContent = response.Content.ReadAsStringAsync().Result;
                return responseContent;
            }

        }



    }
}
