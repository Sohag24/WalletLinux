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
        string privateKeyText = "-----BEGIN PRIVATE KEY-----\r\nMIIJQQIBADANBgkqhkiG9w0BAQEFAASCCSswggknAgEAAoICAQCcJcY8CBXzvJfo\r\n8gBe46Xu24ensEjE1cxbL0v7uTr175UYnxQgidoEB7CQdalhUbbB8i8tl0zr3kNo\r\nZOz4Fvs6k6VTp66LeUsATc/6AqBTi/F1M3Ooq1GYXGKdAsSlS0AX13dkJCVnmsm/\r\nfCRQxez7B3yucP+B2RhBAvAWEDOaQjetYcu2pkD5BfY4iVLbUGDqCB+s/jBP0LHA\r\nlxpHB32I1bBBIit5zxBcu8ivhOheZqL0rMvdxnBX7Y+t+2gGvcGzOOygUOGoRk3e\r\nrgKI18JJM3VvYRXiIj3ned8F1pmLyLlOx39CZzWFFJD5Gr1BPS+rxoshovYBEU0G\r\npvp4z58blsNxWm09YPBeTTzipCvw8397M5PlYILLA8To+HdyGp/6u/fM1230TF1S\r\n4t9O3s/0WnlJLdILF9HZ+lHjkjlQD+n8PWD4Vy/41iC76cK1EMGhfbKE+1hQqCGm\r\ncdLGdcza0f/CilTG7cxZyP3q6ViwiGNcmG9DszO/iEPHn6xQMjoLmDyq8aCw1d5f\r\nNY4bYb5/EG7o/niU5ivyYqvllUx5yLYJlNfs+LVlvtW6c+iQuPD40DZ2Kc9hwUU4\r\nl2rZ33bENUOxsYObja7EndOdkodVHv83W9VuykZyuCtAUSSQuesrFCajWVVR4NeW\r\nZZozJZWQdmfIv/Ndefyte5+VygRg9wIDAQABAoICAEsqwgFvEQY6sNQh/zyukNfp\r\nlSWLrpQWqZ/xpj3AvtnaYecu2RUK10keXIhGI+ol2HkHuV9UmOJopgFwqESLZ5gc\r\nsDcTnuokUw8L5kTfk8d0NCGryY8WHZrfSld0GC7MCUgQ4Lfz0I/NyzXjMGYzyW9U\r\nfNhKrC0ZUVwmamXsmbK2ErBjxhmgceGO1acEsD7ENEsk6v6vhJk/iPoBo1QzDmLb\r\nH1RTeyNRovpJKYQY0nVzKhnnvuH9X2JsiUCDKGIqYb8BdaN9HAwR//VkxfKTDsOH\r\nQo6B6XNo8WznVY2tb1ADqfT7A093eKOeMWXBUf7AUhz6YEqqdIsyBWbg5e1XYSs5\r\nyQ7+aXIRuo0Wtw0ZmDQnU/mdFa6xQI0jzPu9tBO/0O9FjpnvPj0du4S/29Q5l0MI\r\nklZtbXzsqfGN3wTB4icxEeaQcpp9+cWyBRDXAJaHHGUK99g3bT/YT1rYPOnCyqlH\r\nOsN14M9uoTAilqCAkMeUIEB2KGrg+A6uSrrN1iuZI12j/flJ+jdd7svCp8uJOwVb\r\nH0n23eZBmPVflWTO+Ofr4ocFeY6kO9BjHPOEFdLnCWS2IsqrDE5itf1IDeZQm3Ib\r\nJUiyEjFl4zmbHncZSOUci4G7s3e0XijuojStXmp+1BfmxX9VRjLLQg22ylOgtRdJ\r\n27E/oiCKD9rB0rPFrfWhAoIBAQDF+pGwPjmxfCaPSavpH+6ToWlDKKYMiODZfrT+\r\nS5j4pVg4xcneGhqD0qbdvlBVZg5CDJFnk2i9Ov97tT5bo5eNfpWmxgP6miGGCelA\r\nPTzx9R1t/O5sgUn+XQ7Q7NgAfELEEUkkJYHjdprPlM18uvf9WlT6yCpdlScuM4vZ\r\n/zmkYAwFllBwjt5tw0Rs9YZHkQJZsDPIf7x+LKhY196kzdHsw6BSpIrlD1D4m18z\r\nIxMURAAvshm8bpZJMZR54h597MMIj0U3+s1VMogjwbhl+DdZWtzPGrsJwCtDA0ce\r\niL3e0skdOKsl0N+ratngnAd5YJ5segHpAB1pwyc3OEumd+fhAoIBAQDJ6M1MQT5m\r\ngPR1YpUmUWkWYrnqrfmB5vr82CEvxeTgEU5VNBW+d7ncdznJBSkw9SzHLOKe7TTc\r\nGPzktj3RtdVOU+udS1kKo5ThloSJCzAKrwbMkR/IslBZsUljSWxaWNFDMM8Ci2xm\r\ncx0/BMU58FZe5uU7Pb14olGGlIoa2JURlR3AdZEiqFCDQ4V2C/9REVRFIwji3VL0\r\nkidTAroUfzuRpaHzUbGf8tr80crGMwY7Rzi6KbD6PN1HvFRH1V7NgBEQ7Pum7HfM\r\n+Kr+T6kzrwHQZZTxp9xvdTMi9p5a+TTQ6CLX0cZfO+LD4DjY0LGPvlPvvELgBvqu\r\n1eA+SAtCuQPXAoIBADEq7YurqIBfqOPlFACtoX6gk5fOPI7YX58kTKbq926pAaP3\r\n2w6BxRv/8NkZLrJGyju1C1eB0H1Obay3cmkn07ecPGZSvyI5rSGwzHDbxIsCqPBr\r\n5HqmEU5OpiNU7sAQHDS9ZkDH394x0njDJ9VWScht6rTtiTf8luJugsj9Db+46sNo\r\ndUm+8Zao7BFzWInlhYPosrOUpadgR97ona6oPv5iByr/Gk+Q4jcbvyTgDRj3WmR+\r\nAs49WK2M1qb+dTwo1LcHfdVNQDlsd0sKDVWls3lJi5UTumfNhfD6m2sF4sG376KG\r\nNFsL6MCVpM6XcXO/fxjJpDfNg4CEqvSX0pHhLYECggEAdQHnbyhkHAzhHeGaY5FM\r\nH//ok9psPF551ur9GG1LPJpq1a+QZkeFuwCQyTCndUqvv2EAl28Jcf89FuGRo6UH\r\nTxQM96OBn0u67JxIhA6qsZZKj2QOkkTbkY7O463aD+Vmp9RyMpxPnnNZYHrIPNJN\r\ni0OtVg5qba1LJ32PJw2aHY1Xp3+6MuZtehQ4FHfRZs5vEn3CGY5/E2JbTZzH7zbo\r\nc3gUCSivkeywAVEmfIN3OgUFF8UvXWTGK1s1UEeTX+22diOPlmV066CznxrVCDGE\r\nEJu9m+lWMnUtxDY9AKVoeSP9hSp5FhljSXX4G5WdQJrgk457Bs6kmYsyH4WqmOkV\r\nEwKCAQAGILkZd3Eyq72v6ImEJo2smmecx8Q4xEMrv0rroT+n2+mThwTGe0bTpseb\r\nKbN82XJhud4rHz2D6Dc+quwkXBnXBUVYfY697fNFnym/hUp06z+jiQSpemJKPMWQ\r\nI9SjGoyaWGXIhVYXmDOl1siaYMzpJu3Ayia5ns5h3PD3p4ZFa/4212gSmjZlDDM3\r\n90iDdvCRyrPBJmU3IMF/g50QCd55bhTF2SXv+rcoIiKoddY1g6tDKfvSbO/YFpl/\r\nzwwkFiCvjfWdD7J6bhJXnIU3ZkiqmDRPr0eL4qhq/TIKjin2mbmDZi9iL1A+8VWI\r\nKLuWu+8bThAYIhLgGxd4mkI/PzI6\r\n-----END PRIVATE KEY-----\r\n";
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