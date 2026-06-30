using System.Collections.Generic;

namespace JobMore.Models
{
    /// <summary>
    /// 지원 건 하나 = 하나의 레코드. 회사/직무 + 전형 단계 + 일정(마감·면접) + 메모.
    /// 수시/이직 맥락을 담아 채용 경로·연봉·연락처까지 관리한다.
    /// </summary>
    public class Application
    {
        public int Id { get; set; }

        public string Company { get; set; } = string.Empty;     // 회사명
        public string Position { get; set; } = string.Empty;    // 직무
        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
        public Channel Channel { get; set; } = Channel.JobPosting;
        public Priority Priority { get; set; } = Priority.Medium;
        public Stage Stage { get; set; } = Stage.Interested;

        public string Location { get; set; } = string.Empty;    // 근무지
        public string ExpectedSalary { get; set; } = string.Empty; // 희망/제시 연봉(자유 입력)
        public string JobUrl { get; set; } = string.Empty;      // 공고 링크
        public string Contact { get; set; } = string.Empty;     // 담당자/헤드헌터 연락처
        public string Memo { get; set; } = string.Empty;
        public string ColorHex { get; set; } = string.Empty;   // 회사 색(캘린더 표시용)

        public DateTime AddedDate { get; set; } = DateTime.Today;        // 등록일
        public DateTime? AppliedDate { get; set; }                       // 지원일
        public DateTime? Deadline { get; set; }                          // 서류 마감
        public DateTime? NextEventDate { get; set; }                     // 다음 일정(면접 등)
        public string NextEventLabel { get; set; } = string.Empty;       // 다음 일정 설명

        // 단계 변경 이력 (퍼널/타임라인용)
        public List<StageLog> StageLogs { get; set; } = new();
    }
}
