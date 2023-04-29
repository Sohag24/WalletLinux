using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WalletApp.Helper;

public class JwtGenerator
{
  
    public static string GenerateJwtToken(string uri, string ApiMethod, string body,string key)
    {
        var UTCDateTime = GetUTCDateTime();

        int iats = (int)(UTCDateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        int exps = (int)(UTCDateTime.AddSeconds(30).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[]
        {
        new Claim("sub", key),
        new Claim("nonce",Guid.NewGuid().ToString()),
        new Claim("iat",iats.ToString(),ClaimValueTypes.Integer),
        new Claim("exp",exps.ToString(),ClaimValueTypes.Integer),
        new Claim("uri",uri)
        //new Claim("bodyHash",GetBase64Sha256Hash(body))
        });

        if (ApiMethod == ApiMethods.Post)
        {
            claimsIdentity.AddClaim(new Claim("bodyHash", GetBase64Sha256Hash(body)));
        }

        // Load the private key from a file or some other secure storage location
        var privateKey = LoadPrivateKey();

        // Create the signing credentials using the RSA256 algorithm and the private key
        var signingCredentials = new SigningCredentials(new RsaSecurityKey(privateKey), SecurityAlgorithms.RsaSha256);

        var tokenHandler = new JwtSecurityTokenHandler();

        //create the jwt
        var token = (JwtSecurityToken)
                tokenHandler.CreateJwtSecurityToken(issuer: "Sam", audience: "sohag",
                    subject: claimsIdentity, notBefore: UTCDateTime,issuedAt:UTCDateTime, expires: UTCDateTime.AddSeconds(30), signingCredentials: signingCredentials);
        var tokenString =tokenHandler.WriteToken(token);

        return tokenString;

    }

    private static string GetBase64Sha256Hash(string input)
    {
        string hashString;
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.Default.GetBytes(input));
            hashString = ToHex(hash, false);
        }

        return hashString;
    }

    private static string ToHex(byte[] bytes, bool upperCase)
    {
        StringBuilder result = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
        return result.ToString();
    }


    private static RSA LoadPrivateKey()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var filePath = Path.Combine(currentDirectory, "Helper", "privatekey.pem");

        // Replace this path with the path to your private key file
        string privateKeyFilePath = filePath;//"D:/privatekey.pem";

        // Read the contents of the private key file
        string privateKeyText = "-----BEGIN PRIVATE KEY-----\r\n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\r\niLG15UJPY0pJhGq8cHB1wjsFi/5G6g==\r\n-----END PRIVATE KEY-----";//File.ReadAllText(privateKeyFilePath);

        // Create a new RSA key object to hold the private key
        RSA privateKey = RSA.Create();

        // Load the private key from the file
        privateKey.ImportFromPem(privateKeyText);

        return privateKey;
    }

    public static DateTime GetUTCDateTime()
    {
        /*
        var httpClient = new HttpClient();

        // Set the URL of the World Time API endpoint
        var apiUrl = "http://worldtimeapi.org/api/timezone/Asia/Dhaka"; // Change

        // Send an HTTP GET request to the API endpoint and get the response
        var response =  httpClient.GetAsync(apiUrl).Result;

        // Read the response content as a string
        var responseContent = response.Content.ReadAsStringAsync().Result;

        var jsonObject = JObject.Parse(responseContent); 
        
        // Get the value of the "name" variable as a string
        var utc_datetime = (DateTime)jsonObject["utc_datetime"];
        */

        // Parse the response JSON to get the current UTC datetime
        //var dateTimeUtc = JsonConvert.DeserializeObject<string>(responseContent);

        var utc_datetime = DateTime.UtcNow;

        return utc_datetime;

    }

}