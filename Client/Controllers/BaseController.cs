using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Client.Controllers
{
    public class BaseController : Controller
    {
        private readonly IHttpClientFactory _factory;

        public BaseController(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        private HttpClient CreateClient()
        {
            var client = _factory.CreateClient("API");

            var token = Request.Cookies["Auth.JWT"];

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        protected async Task<T> GetAsync<T>(string endpoint)
        {
            var client = CreateClient();

            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new Exception(
                    $"[{(int)response.StatusCode}] {response.StatusCode}\n" +
                    $"Endpoint: {endpoint}\n" +
                    $"Response: {error}"
                );
            }

            return await response.Content.ReadFromJsonAsync<T>();
        }


        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            var client = CreateClient();

            var response = await client.PostAsJsonAsync(endpoint, data);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        protected async Task<bool> PutAsync<TRequest>(string endpoint, TRequest data)
        {
            var client = CreateClient();

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }

        protected async Task<bool> DeleteAsync(string endpoint)
        {
            var client = CreateClient();

            var response = await client.DeleteAsync(endpoint);

            return response.IsSuccessStatusCode;
        }
        
        protected async Task<bool> UploadFileAsync(string endpoint, IFormFile file)
        {
            var client = CreateClient(); 
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            
            content.Add(streamContent, "file", file.FileName);
            var response = await client.PostAsync(endpoint, content);
    
            return response.IsSuccessStatusCode;
        }

        protected async Task<TResponse?> PostMultipartFormAsync<TResponse>(string endpoint, Dictionary<string, string> formFields, IFormFile? file = null, string fileFieldName = "cvFile")
        {
            var client = CreateClient();
            using var content = new MultipartFormDataContent();

            foreach (var field in formFields)
            {
                content.Add(new StringContent(field.Value ?? ""), field.Key);
            }

            if (file != null && file.Length > 0)
            {
                var fileStream = file.OpenReadStream();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                content.Add(streamContent, fileFieldName, file.FileName);
            }

            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
    }
}
