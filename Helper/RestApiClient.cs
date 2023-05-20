using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class RestApiClient
{
    private readonly HttpClient httpClient;

    public RestApiClient(string baseUrl)
    {
        this.httpClient = new HttpClient();
        this.httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<string> GetAsync(string endpoint, string authorizationToken, string headerName, string headerValue)
    {
        AddAuthorizationHeader(authorizationToken);
        AddCustomHeader(headerName, headerValue);

        HttpResponseMessage response = await httpClient.GetAsync(endpoint);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API call failed with status code {response.StatusCode}. Response body: {responseBody}");
        }

        return responseBody;
    }

    public async Task<string> PostAsync(string endpoint, string authorizationToken, string headerName, string headerValue, string body)
    {
        AddAuthorizationHeader(authorizationToken);
        AddCustomHeader(headerName, headerValue);
 
        HttpContent content = new StringContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpResponseMessage response = await httpClient.PostAsync(endpoint, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API call failed with status code {response.StatusCode}.  Token: {authorizationToken}  Response body: {responseBody}");
            //throw new Exception($"{ responseBody }");
            //return responseBody;
        }

        return responseBody;
    }

    public async Task<string> PutAsync(string endpoint, string authorizationToken, string headerName, string headerValue, string body)
    {
        AddAuthorizationHeader(authorizationToken);
        AddCustomHeader(headerName, headerValue);

        HttpContent content = new StringContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpResponseMessage response = await httpClient.PutAsync(endpoint, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API call failed with status code {response.StatusCode}. Response body: {responseBody}");
        }

        return responseBody;
    }

    public async Task<string> DeleteAsync(string endpoint, string authorizationToken, string headerName, string headerValue)
    {
        AddAuthorizationHeader(authorizationToken);
        AddCustomHeader(headerName, headerValue);

        HttpResponseMessage response = await httpClient.DeleteAsync(endpoint);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API call failed with status code {response.StatusCode}. Token: {authorizationToken} Response body: {responseBody}");
        }

        return responseBody;
    }

    private void AddAuthorizationHeader(string authorizationToken)
    {
        if (!string.IsNullOrEmpty(authorizationToken))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken);
        }
    }

    private void AddCustomHeader(string headerName, string headerValue)
    {
        if (!string.IsNullOrEmpty(headerName) && !string.IsNullOrEmpty(headerValue))
        {
            httpClient.DefaultRequestHeaders.Add(headerName, headerValue);
        }
    }
}
