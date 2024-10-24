using System.Net.Http.Headers;

namespace ArchiverBot;

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ConfluenceService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _spaceKey;
    private readonly string _pageId;
    private readonly string _authToken;

    public ConfluenceService(string baseUrl, string spaceKey, string pageId, string apiToken, string email)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
        _spaceKey = spaceKey;
        _pageId = pageId;
        _authToken = apiToken;
        
        var credentials = $"{email}:{apiToken}";
        var base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
    }

    public async Task PostToConfluence(string title, string content)
    {
        var url = $"{_baseUrl}/wiki/rest/api/content/{_pageId}";
        
        var body = new
        {
            version = new { number = 2 }, 
            title = title,
            type = "page",
            body = new
            {
                storage = new
                {
                    value = content,
                    representation = "storage"
                }
            }
        };

        var jsonBody = JsonConvert.SerializeObject(body);
        var contentMessage = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(url, contentMessage);
        
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Successfully posted to Confluence!");
        }
        else
        {
            Console.WriteLine($"Failed to post to Confluence: {response.StatusCode}");
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine(error);
        }
    }
}
