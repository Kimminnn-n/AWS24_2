using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class OpenAIResponse
{
    public Choice[] Choices { get; set; }
}

public class Choice
{
    public Message Message { get; set; }
}

public class Message
{
    public string Role { get; set; }
    public string Content { get; set; }
}

class Program
{
    private static readonly string apiKey = "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";
    private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

    static async Task Main(string[] args)
    {
        Console.WriteLine("ChatGPT 챗봇에 오신 것을 환영합니다!");
        Console.WriteLine("종료하려면 'exit'을 입력하세요.\n");

        while (true)
        {
            Console.Write("You: ");
            string userInput = Console.ReadLine();

            if (userInput?.ToLower() == "exit") break;

            string response = await GetChatGPTResponse(userInput);
            Console.WriteLine($"ChatGPT: {response}\n");
        }
    }

    static async Task<string> GetChatGPTResponse(string prompt)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = "gpt-4",  // 또는 "gpt-3.5-turbo"
                messages = new[]
                {
                new { role = "system", content = "You are a helpful assistant." },
                new { role = "user", content = prompt }
            },
                max_tokens = 1000
            };

            string json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // API 호출을 보내고 응답 받기
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            // 상태 코드가 성공적인지 확인
            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseJson);

                // "choices"와 그 하위 값 처리
                if (responseData.TryGetProperty("choices", out JsonElement choices) && choices.GetArrayLength() > 0)
                {
                    string message = choices[0].GetProperty("message").GetProperty("content").GetString();
                    return message?.Trim() ?? "응답이 없습니다.";
                }
                else
                {
                    return "응답이 비어 있습니다.";
                }
            }
            else
            {
                // 상태 코드가 실패인 경우
                Console.WriteLine($"Error: {response.StatusCode}");
                return $"API 요청 실패: {response.StatusCode}";  // 실패 메시지 출력
            }
        }
    }
};
