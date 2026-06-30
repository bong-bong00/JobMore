namespace JobMore.Models
{
    /// <summary>자기소개서 항목. 작성 탭에서 만들고, 지원 시 복붙해서 가져간다.</summary>
    public class CoverLetter
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;     // 제목(예: 토스 지원동기)
        public string Question { get; set; } = string.Empty;  // 문항(예: 지원 동기를 기술하시오)
        public string Content { get; set; } = string.Empty;   // 작성 내용
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int CharCount => (Content ?? string.Empty).Length;  // 글자 수(자소서는 글자수 제한이 흔함)
    }
}
