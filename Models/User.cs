namespace JobMore.Models
{
    /// <summary>회원(단일 사용자). 첫 실행 시 가입하고, 설정에서 수정한다.</summary>
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;     // 이름
        public int Age { get; set; }                          // 나이
        public string Email { get; set; } = string.Empty;    // 이메일(로그인 ID)
        public string Password { get; set; } = string.Empty; // 비밀번호(과제용 평문 저장)
        public string Phone { get; set; } = string.Empty;    // 연락처
        public string DesiredJob { get; set; } = string.Empty;// 희망 직무
        public string ApiKey { get; set; } = string.Empty;   // Gemini API 키(본인 PC에만 저장)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
