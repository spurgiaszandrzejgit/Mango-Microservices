using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Web.Services
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BaseService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ResponseDTO<T>> SendAsync<T>(ApiRequest apiRequest)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MangoAPI");

                using var message = new HttpRequestMessage
                {
                    RequestUri = new Uri(apiRequest.Url),
                    Method = apiRequest.ApiType switch
                    {
                        SD.ApiType.POST => HttpMethod.Post,
                        SD.ApiType.PUT => HttpMethod.Put,
                        SD.ApiType.DELETE => HttpMethod.Delete,
                        _ => HttpMethod.Get
                    }
                };

                message.Headers.Add("Accept", "application/json");

                if (!string.IsNullOrWhiteSpace(apiRequest.AccessToken))
                {
                    message.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiRequest.AccessToken);
                }

                if (apiRequest.Data != null)
                {
                    message.Content = new StringContent(
                        JsonConvert.SerializeObject(apiRequest.Data),
                        Encoding.UTF8,
                        "application/json");
                }

                var response = await client.SendAsync(message);
                var content = await response.Content.ReadAsStringAsync();
                    
                var result = JsonConvert.DeserializeObject<ResponseDTO<T>>(content);

                if (result == null)
                {
                    return new ResponseDTO<T>
                    {
                        IsSuccess = false,
                        DisplayMessage = "Error",
                        ErrorMessages = new List<string> { "Empty/invalid response from API." }
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ResponseDTO<T>
                {
                    IsSuccess = false,
                    DisplayMessage = "Error",
                    ErrorMessages = new List<string> { ex.Message }
                };
            }
        }
    }
}