using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WebApplication2.Helper;
//using WebApplication2.Migrations;

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
        string privateKeyText = File.ReadAllText(privateKeyFilePath);

        // Create a new RSA key object to hold the private key
        RSA privateKey = RSA.Create();

        // Load the private key from the file
        privateKey.ImportFromPem(privateKeyText);

        return privateKey;
    }

    public static DateTime GetUTCDateTime()
    {
        var httpClient = new HttpClient();

        // Set the URL of the World Time API endpoint
        var apiUrl = "http://worldtimeapi.org/api/timezone/Asia/Dhaka";

        // Send an HTTP GET request to the API endpoint and get the response
        var response =  httpClient.GetAsync(apiUrl).Result;

        // Read the response content as a string
        var responseContent = response.Content.ReadAsStringAsync().Result;

        var jsonObject = JObject.Parse(responseContent);

        // Get the value of the "name" variable as a string
        var utc_datetime =(DateTime)jsonObject["utc_datetime"];


        // Parse the response JSON to get the current UTC datetime
        //var dateTimeUtc = JsonConvert.DeserializeObject<string>(responseContent);

        return utc_datetime;

    }

}