using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobMore.Services
{
    /// <summary>
    /// 자소서 생성 서비스.
    /// - API 키가 있으면 Google Gemini REST API로 실제 생성.
    /// - 키가 없거나 실패하면 켜둔 항목으로 '초안 조립'(오프라인 폴백).
    /// </summary>
    public static class AiService
    {
        // ┌─────────────────────────────────────────────────────────────┐
        // │  여기에 Gemini API 키를 넣으세요. (Google AI Studio에서 무료 발급)  │
        // │  비워두면 자동으로 '초안 조립'(오프라인)으로 동작합니다.            │
        // │  ⚠ 발표/제출 후에는 이 키를 폐기(재발급)하세요.                    │
        // └─────────────────────────────────────────────────────────────┘
        private const string GeminiApiKey = "AQ.Ab8RN6KVvQ2HyJOq6uEaC5NkVDjcc64WXOsM1o2MOu3fM4zT7A";   // 예: "AIzaSy...."

        // 무료 등급에서 쓰기 좋은 모델. 필요 시 여기만 바꾸면 됨.
        private const string Model = "gemini-2.5-flash";
        private const string Endpoint =
            "https://generativelanguage.googleapis.com/v1beta/models/" + Model + ":generateContent";

        /// <summary>프롬프트 구성 + 생성. 키 없으면 폴백 초안.</summary>
        public static async Task<string> GenerateCoverLetterAsync(
            string company, string question, IEnumerable<string> includedItems)
        {
            var items = new List<string>(includedItems);
            string prompt = BuildPrompt(company, question, items);

            if (string.IsNullOrWhiteSpace(GeminiApiKey))
                return BuildDraft(company, question, items);

            try
            {
                return await CallGeminiAsync(GeminiApiKey, prompt);
            }
            catch
            {
                return BuildDraft(company, question, items)
                    + "\n\n(※ API 호출에 실패하여 자동 초안으로 작성되었습니다. 키/네트워크를 확인하세요.)";
            }
        }

        private static string BuildPrompt(string company, string question, List<string> items)
        {
            var sb = new StringBuilder();
            sb.AppendLine("당신은 한국 취업 자기소개서 작성을 돕는 전문가입니다.");
            sb.AppendLine("아래 지원자 정보를 자연스럽게 녹여, 진솔하고 구체적인 자기소개서 한 편을 한국어로 작성하세요.");
            sb.AppendLine("과장 없이, 700자 내외로, 문단을 나눠서 작성하세요.");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(company))
                sb.AppendLine($"[지원 회사] {company}");
            if (!string.IsNullOrWhiteSpace(question))
                sb.AppendLine($"[문항] {question}");
            sb.AppendLine("[지원자가 사용을 허락한 항목]");
            if (items.Count == 0)
                sb.AppendLine("- (선택된 항목 없음 — 일반적인 강점 위주로 작성)");
            else
                foreach (var it in items) sb.AppendLine($"- {it}");
            return sb.ToString();
        }

        private static async Task<string> CallGeminiAsync(string apiKey, string prompt)
        {
            using var http = new HttpClient();
            var bodyObj = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };
            string json = JsonSerializer.Serialize(bodyObj);

            using var req = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            req.Headers.Add("x-goog-api-key", apiKey);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var resp = await http.SendAsync(req);
            string respText = await resp.Content.ReadAsStringAsync();
            resp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(respText);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            return text ?? string.Empty;
        }

        /// <summary>키 없이 켜둔 항목을 문장 틀에 끼워 만든 초안.</summary>
        private static string BuildDraft(string company, string question, List<string> items)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(company))
                sb.Append($"저는 {company}에 지원하는 지원자입니다. ");
            else
                sb.Append("저는 해당 직무에 지원하는 지원자입니다. ");

            if (items.Count > 0)
            {
                sb.Append("그동안 ");
                sb.Append(string.Join(", ", items));
                sb.Append(" 등을 통해 역량과 경험을 쌓아왔습니다. ");
            }
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("이러한 경험을 바탕으로 입사 후에도 맡은 일에 책임감 있게 임하며 성장하는 인재가 되겠습니다.");
            sb.AppendLine();
            sb.AppendLine("(※ 이 글은 켜둔 항목으로 만든 자동 초안입니다. 설정에서 API 키를 입력하면 더 자연스러운 글이 생성됩니다. 내용은 직접 다듬어 사용하세요.)");
            return sb.ToString();
        }
    }
}
