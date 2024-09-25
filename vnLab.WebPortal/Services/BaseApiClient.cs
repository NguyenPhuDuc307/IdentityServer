using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using vnLab.ViewModel;

namespace vnLab.WebPortal.Services
{
    public class BaseApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BaseApiClient(IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<T>> GetListAsync<T>(string url, bool requiredLogin = false)
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            client.BaseAddress = new Uri(_configuration["BackendApiUrl"]!);
            if (requiredLogin)
            {
                var token = await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<T>>(body);
            return data!;
        }

        public async Task<Pagination<T>> GetPagingAsync<T>(string url, bool requiredLogin = false) where T : class
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            client.BaseAddress = new Uri(_configuration["BackendApiUrl"]!);
            if (requiredLogin)
            {
                var token = await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Pagination<T>>(body);
            return data!;
        }

        public async Task<T> GetAsync<T>(string url, bool requiredLogin = false)
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            client.BaseAddress = new Uri(_configuration["BackendApiUrl"]!);
            if (requiredLogin)
            {
                var token = await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<T>(body);
            return data!;
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest requestContent, bool requiredLogin = true)
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            client.BaseAddress = new Uri(_configuration["BackendApiUrl"]!);
            StringContent httpContent = null!;
            if (requestContent != null)
            {
                var json = JsonConvert.SerializeObject(requestContent);
                httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            }

            if (requiredLogin)
            {
                var token = await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var response = await client.PostAsync(url, httpContent);
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TResponse>(body)!;
            }
            throw new Exception(body);
        }

        public async Task<bool> PutAsync<TRequest, TResponse>(string url, TRequest requestContent, bool requiredLogin = true)
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            client.BaseAddress = new Uri(_configuration["BackendApiUrl"]!);
            HttpContent httpContent = null!;
            if (requestContent != null)
            {
                var json = JsonConvert.SerializeObject(requestContent);
                httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            }

            if (requiredLogin)
            {
                var token = await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var response = await client.PutAsync(url, httpContent);
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return true;

            throw new Exception(body);
        }

        public async Task<bool> DeleteAsync<TResponse>(string url, bool requiredLogin = true)
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            client.BaseAddress = new Uri(_configuration["BackendApiUrl"]!);

            if (requiredLogin)
            {
                var token = await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return true;

            throw new Exception(body);
        }
    }
}