using Graduation_Project_Management.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {

        private readonly HttpClient _httpClient;
        private const string PythonApiUrl = "http://localhost:8000/ask";

        public ChatController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public class QuestionRequest
        {
            public string Question { get; set; }
        }

        public class QuestionResponse
        {
            [JsonPropertyName("answer")]
            public string Answer { get; set; }

            [JsonPropertyName("tokens_used")]
            public int TokensUsed { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> AskQuestion([FromBody] QuestionRequest request)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(new { question = request.Question }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(PythonApiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<QuestionResponse>(responseContent);

                return Ok(result);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                return StatusCode(429, "Quota exceeded. Please try again later or consider upgrading your plan.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error communicating with Python API: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
