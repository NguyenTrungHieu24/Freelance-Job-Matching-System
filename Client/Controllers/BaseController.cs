using Client.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

        protected CurrentUserViewModel GetCurrentUser()
        {
            var jwt = HttpContext.Session.GetString("Auth.JWT");
            var role = HttpContext.Session.GetString("Auth.Role");
            var userJson = HttpContext.Session.GetString("Auth.User");

            CurrentUser? user = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                try
                {
                    user = JsonSerializer.Deserialize<CurrentUser>(userJson);
                }
                catch
                {
                    // log nếu cần
                }
            }

            return new CurrentUserViewModel
            {
                Jwt = jwt,
                Role = role,
                Info = user
            };
        }

        private HttpClient CreateClient()
        {
            var client = _factory.CreateClient("API");

            var token = HttpContext.Session.GetString("Auth.JWT");

            if (!string.IsNullOrEmpty(token))
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
                throw new Exception(error);
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
    }
}
