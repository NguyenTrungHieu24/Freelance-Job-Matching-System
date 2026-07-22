using Client.Models.Auth;
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

        protected async Task PutOrThrowAsync<TRequest>(string endpoint, TRequest data)
        {
            var client = CreateClient();

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
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

        protected async Task<string?> UploadJobImageAsync(string endpoint, IFormFile file)
        {
            var client = CreateClient(); 
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);
            
            var response = await client.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent);
                if (jsonDoc.RootElement.TryGetProperty("location", out var locationProp))
                {
                    return locationProp.GetString();
                }
            }
            return null;
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



    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            var success = await PutAsync("api/auth/change-password", new
            {
                OldPassword = model.OldPassword,
                NewPassword = model.NewPassword,
                ConfirmPassword = model.ConfirmPassword
            });

            if (success)
            {
                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction("ChangePassword");
            }

            TempData["Error"] = "Failed to change password";
            return RedirectToAction("ChangePassword");
        }
        catch (Exception e)
        {
            TempData["Error"] = ParseErrorMessage(e.Message);
            return RedirectToAction("ChangePassword");
        }
    }

        protected string ParseErrorMessage(string rawError)
        {
            if (string.IsNullOrWhiteSpace(rawError))
                return "Đã xảy ra lỗi không xác định.";

            if (!rawError.Trim().StartsWith("{"))
                return rawError;

            try
            {
                using var doc = JsonDocument.Parse(rawError);
                var root = doc.RootElement;

                if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    var sb = new StringBuilder();
                    foreach (var prop in errorsProp.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in prop.Value.EnumerateArray())
                            {
                                var msg = item.GetString();
                                if (!string.IsNullOrEmpty(msg))
                                {
                                    if (msg.Contains("Password must be at least 6 characters"))
                                        msg = "Mật khẩu phải chứa ít nhất 6 ký tự.";
                                    else if (msg.Contains("Email already exists") || msg.Contains("Email already in use"))
                                        msg = "Địa chỉ email đã tồn tại hoặc đang được sử dụng.";
                                    
                                    sb.AppendLine(msg);
                                }
                            }
                        }
                        else
                        {
                            var msg = prop.Value.GetString();
                            if (!string.IsNullOrEmpty(msg))
                            {
                                sb.AppendLine(msg);
                            }
                        }
                    }
                    if (sb.Length > 0)
                        return sb.ToString().Trim();
                }

                if (root.TryGetProperty("message", out var messageProp))
                {
                    return messageProp.GetString() ?? "Đã xảy ra lỗi.";
                }

                if (root.TryGetProperty("title", out var titleProp))
                {
                    return titleProp.GetString() ?? "Đã xảy ra lỗi.";
                }
            }
            catch
            {
                // ignore
            }

            return rawError;
        }
    }
}
