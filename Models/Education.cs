namespace JobMore.Models
{
    /// <summary>학력 항목 (자소서 삽입·AI 포함 대상).</summary>
    public class Education
    {
        public int Id { get; set; }
        public string School { get; set; } = string.Empty;   // 학교
        public string Major { get; set; } = string.Empty;    // 전공
        public string Period { get; set; } = string.Empty;   // 기간(예: 2020.03~2024.02)
        public string Note { get; set; } = string.Empty;     // 비고(학점 등)
    }
}
