using System.ComponentModel;

namespace JobMore.Models
{
    /// <summary>
    /// 전형 단계 파이프라인 (수시/이직 포함 일반형)
    /// 관심 → 지원 → 서류합격 → 1차면접 → 2차면접 → 처우협의 → 최종합격 / 불합격 / 중도포기
    /// </summary>
    public enum Stage
    {
        [Description("관심")]        Interested,
        [Description("지원")]        Applied,
        [Description("서류합격")]    DocumentPassed,
        [Description("1차면접")]     FirstInterview,
        [Description("2차면접")]     SecondInterview,
        [Description("처우협의")]    Negotiation,
        [Description("최종합격")]    Offer,
        [Description("불합격")]      Rejected,
        [Description("중도포기")]    Withdrawn
    }

    /// <summary>고용 형태</summary>
    public enum EmploymentType
    {
        [Description("정규직")]      FullTime,
        [Description("계약직")]      Contract,
        [Description("인턴")]        Intern,
        [Description("프리랜스")]    Freelance
    }

    /// <summary>지원 경로 (수시 채용 맥락)</summary>
    public enum Channel
    {
        [Description("공고지원")]    JobPosting,
        [Description("헤드헌터")]    Headhunter,
        [Description("지인추천")]    Referral,
        [Description("직접지원")]    Direct,
        [Description("채용제안")]    InboundOffer
    }

    /// <summary>관심도</summary>
    public enum Priority
    {
        [Description("높음")]        High,
        [Description("보통")]        Medium,
        [Description("낮음")]        Low
    }
}
