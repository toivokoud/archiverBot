using System.Runtime.InteropServices.JavaScript;

namespace ArchiverBot;

using System.Net.Http.Headers;
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

        // Set authorization header
        var credentials = $"{email}:{apiToken}";
        var base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
    }

    public async Task PostToConfluence(string title, string content)
    {
        // Get current page content and version
        var page = await GetCurrentPage();
        
        if (page == null)
        {
            Console.WriteLine("Failed to retrieve the current page.");
            return;
        }

        int newVersion = page.version.number + 1;
        string prevContent = page.body.storage.value;

        // Concatenate the new content with the previous content
        string aggregatedContent = $"{prevContent}<br/><br/>{content}{DateTime.Now}";

        var url = $"{_baseUrl}/wiki/rest/api/content/{_pageId}";

        var body = new
        {
            version = new { number = newVersion },
            title = title,
            type = "page",
            body = new
            {
                storage = new
                {
                    value = aggregatedContent,
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
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response from Confluence: {responseContent}");
        }
        else
        {
            Console.WriteLine($"Failed to post to Confluence: {response.StatusCode}");
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine(error);
        }
    }

    // Retrieve the current page content and version
    private async Task<ConfluencePage> GetCurrentPage()
    {
        var url = $"{_baseUrl}/wiki/rest/api/content/{_pageId}?expand=body.storage,version";
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<ConfluencePage>(responseData);
            Console.WriteLine(jsonResponse);
            return jsonResponse;
        }
        else
        {
            Console.WriteLine($"Failed to get current page: {response.StatusCode}");
            return null;
        }
    }
}

public class ConfluencePage
{
    public int id { get; set; }
    public string title { get; set; }
    public VersionInfo version { get; set; }
    public BodyInfo body { get; set; }
}

public class VersionInfo
{
    public int number { get; set; }
}

public class BodyInfo
{
    public Storage storage { get; set; }
}

public class Storage
{
    public string value { get; set; }
    public string representation { get; set; }
}
