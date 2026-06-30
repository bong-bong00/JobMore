namespace JobMore.Models
{
    /// <summary>보유 자격증. 작성 탭에서 '검색'으로 골라 담는다.</summary>
    public class Certificate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;      // 자격증명
        public string Issuer { get; set; } = string.Empty;    // 발급기관
        public DateTime? AcquiredDate { get; set; }           // 취득일
        public string Number { get; set; } = string.Empty;    // 자격증번호(선택)
    }
}
