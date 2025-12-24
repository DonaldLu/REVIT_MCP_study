using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RevitMCP.Core
{
    /// <summary>
    /// Gemini 2.5 Flash API 整合服務
    /// </summary>
    public class GeminiChatService
    {
        private readonly string _apiKey;
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
        private readonly HttpClient _httpClient;

        public GeminiChatService(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// 與 Gemini AI 交互式對話
        /// </summary>
        public async Task<string> ChatAsync(string userMessage, string context = "")
        {
            try
            {
                // 構建請求
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = $"{context}\n\n用戶問題: {userMessage}"
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 1024
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 發送請求到 Gemini API
                var response = await _httpClient.PostAsync(
                    $"{_apiUrl}?key={_apiKey}",
                    content
                );

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Gemini API 錯誤: {response.StatusCode}");
                }

                // 解析回應
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseContent);

                string aiResponse = result.candidates[0].content.parts[0].text;
                return aiResponse;
            }
            catch (Exception ex)
            {
                return $"AI 服務錯誤: {ex.Message}";
            }
        }
    }
}