namespace JobMore.Models
{
    /// <summary>대외활동 항목 (자소서 삽입·AI 포함 대상).</summary>
    public class Activity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;        // 활동명
        public string Organization { get; set; } = string.Empty; // 기관/단체
        public string Period { get; set; } = string.Empty;       // 기간
        public string Description { get; set; } = string.Empty;  // 한 줄 설명
    }
}
