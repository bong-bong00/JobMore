namespace JobMore.Models
{
    /// <summary>희망 직무 항목 (자소서 삽입·AI 포함 대상).</summary>
    public class DesiredJob
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;     // 직무명
        public string Category { get; set; } = string.Empty; // 분류(검색 시)
    }
}
