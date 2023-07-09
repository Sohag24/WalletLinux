using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

public class RestApiClient
{
    private readonly HttpClient httpClient;

    public RestApiClient(string baseUrl)
    {
        this.httpClient = new HttpClient();
        this.httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<JsonResult> GetAsync(string endpoint, string authorizationToken, string headerName, string headerValue)
    {
        AddAuthorizationHeader(authorizationToken);
        AddCustomHeader(headerName, headerValue);

        HttpResponseMessage response = await httpClient.GetAsync(endpoint);
        string responseBody = await response.Content.ReadAsStringAsync();
        var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
        {
            string FullResponse=$"API call failed with status code: {response.StatusCode}, Response body: {responseBody}";
            string messageValue = responseJson.GetProperty("message").GetString() ?? FullResponse;
            return JsonData(null, messageValue);
        }

        return JsonData(responseJson, null);
    }

    public async Task<JsonResult> PostAsync(string endpoint, string authorizationToken, string headerName, string headerValue, string body)
    {
        AddAuthorizationHeader(authorizationToken);
        AddCustomHeader(headerName, headerValue);
 
        HttpContent content = new StringContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpResponseMessage response = await httpClient.PostAsync(endpoint, content);
        string responseBody = await response.Content.ReadAsStringAsync();
        var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
        {
            string FullResponse = $"API call failed with status code: {response.StatusCode}, Response body: {responseBody}";
            string messageValue = responseJson.GetProperty("message").GetString() ?? FullResponse;
            return JsonData(null, messageValue);
        }

        return JsonData(responseJson, null);
    }

    public async Task<JsonResult> PutAsync(string endpoint, string authorizationToken, string headerName, string headerValue, string body)
    {
        AddAuthorizationHeader(authorizationToken);
        AddCustomHeader(headerName, headerValue);

        HttpContent content = new StringContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpResponseMessage response = await httpClient.PutAsync(endpoint, content);
        string responseBody = await response.Content.ReadAsStringAsync();
        var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
        {
            string FullResponse = $"API call failed with status code: {response.StatusCode}, Response body: {responseBody}";
            string messageValue = responseJson.GetProperty("message").GetString() ?? FullResponse;
            return JsonData(null, messageValue);
        }

        return JsonData(responseJson, null);
    }

    public async Task<JsonResult> DeleteAsync(string endpoint, string authorizationToken, string headerName, string headerValue)
    {
        AddAuthorizationHeader(authorizationToken);
        AddCustomHeader(headerName, headerValue);

        HttpResponseMessage response = await httpClient.DeleteAsync(endpoint);
        string responseBody = await response.Content.ReadAsStringAsync();
        //var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
        {
            string FullResponse = $"API call failed with status code: {response.StatusCode}, Response body: {responseBody}";
            //string messageValue = responseJson.GetProperty("message").GetString() ?? FullResponse;
            return JsonData(null, responseBody);
        }

        return JsonData("Delete Successful!", null);
    }


    public async Task<string> GetAsync_String(string endpoint, string authorizationToken, string headerName, string headerValue)
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

    public async Task<string> PostAsync_String(string endpoint, string authorizationToken, string headerName, string headerValue, string body)
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


    public async Task<HttpResponseMessage> GetAsync_Response(string endpoint, string authorizationToken, string headerName, string headerValue)
    {
        AddAuthorizationHeader(authorizationToken);
        AddCustomHeader(headerName, headerValue);

        HttpResponseMessage response = await httpClient.GetAsync(endpoint);

        return response;
    }

    public async Task<HttpResponseMessage> PostAsync_Response(string endpoint, string authorizationToken, string headerName, string headerValue, string body)
    {
        AddAuthorizationHeader(authorizationToken);
        AddCustomHeader(headerName, headerValue);

        HttpContent content = new StringContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpResponseMessage response = await httpClient.PostAsync(endpoint, content);
        
        return response;
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

    public JsonResult JsonData(dynamic? data = null, dynamic? message = null)
    {
        return new JsonResult(new { data = data, message = message });
    }
}
