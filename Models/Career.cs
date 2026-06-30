namespace JobMore.Models
{
    /// <summary>경력 항목 (자소서 삽입·AI 포함 대상).</summary>
    public class Career
    {
        public int Id { get; set; }
        public string Company { get; set; } = string.Empty;  // 회사
        public string Role { get; set; } = string.Empty;     // 직무/직책
        public string Period { get; set; } = string.Empty;   // 기간
        public string Description { get; set; } = string.Empty; // 한 줄 설명
    }
}
