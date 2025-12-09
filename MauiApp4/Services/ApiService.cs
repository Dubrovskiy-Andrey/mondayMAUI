using MauiApp4.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiApp4.Services
{
    internal class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7190/api/Contacts";

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<ContactDto>> GetContactsAsync(string search = null)
        {
            try
            {
                var url = _baseUrl;
                if (!string.IsNullOrEmpty(search))
                {
                    url += $"?Search={Uri.EscapeDataString(search)}";
                }

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ContactDto>>(content) ?? new List<ContactDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения данных: {ex.Message}");
                return new List<ContactDto>();
            }
        }

        public async Task<ContactDto> GetContactAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/{id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ContactDto>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения данных: {ex.Message}");
                throw;
            }
        }

        public async Task<ContactDto> CreateContactAsync(CreateContactDto contact)
        {
            try
            {
                var json = JsonSerializer.Serialize(contact);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ContactDto>(responseContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания контакта: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateContactAsync(int id, UpdateContactDto contact)
        {
            try
            {
                var json = JsonSerializer.Serialize(contact);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/{id}", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления данных: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteContactAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления контакта: {ex.Message}");
                throw;
            }
        }
    }
}
