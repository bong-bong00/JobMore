using System.Collections.Generic;
using JobMore.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMore.Data
{
    /// <summary>
    /// EF Core 코드 퍼스트 DbContext (SQLite).
    /// 서버 설치가 필요 없고, 실행 폴더에 jobtracker.db 파일이 자동 생성됩니다.
    /// </summary>
    public class TrackerDbContext : DbContext
    {
        public DbSet<Application> Applications => Set<Application>();
        public DbSet<StageLog> StageLogs => Set<StageLog>();
        public DbSet<User> Users => Set<User>();
        public DbSet<CoverLetter> CoverLetters => Set<CoverLetter>();
        public DbSet<Certificate> Certificates => Set<Certificate>();
        public DbSet<Education> Educations => Set<Education>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<Career> Careers => Set<Career>();
        public DbSet<DesiredJob> DesiredJobs => Set<DesiredJob>();
        public DbSet<EventType> EventTypes => Set<EventType>();

        // 실행 파일 옆에 DB 파일 생성 (별도 설정 불필요)
        public const string ConnectionString = "Data Source=jobtracker.db";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Application>()
                .HasMany(a => a.StageLogs)
                .WithOne(s => s.Application)
                .HasForeignKey(s => s.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        /// <summary>DB가 없으면 생성하고, 비어 있으면 시드 데이터를 넣는다.</summary>
        public void Initialize()
        {
            Database.EnsureCreated();
            if (Applications.Any()) return;

            var seed = new List<Application>
            {
                new Application
                {
                    Company = "토스", Position = "프론트엔드 개발자",
                    EmploymentType = EmploymentType.FullTime, Channel = Channel.JobPosting,
                    Priority = Priority.High, Stage = Stage.SecondInterview,
                    Location = "서울 강남", ExpectedSalary = "6,000만",
                    AddedDate = DateTime.Today.AddDays(-20), AppliedDate = DateTime.Today.AddDays(-18),
                    NextEventDate = DateTime.Today.AddDays(2), NextEventLabel = "2차 임원면접",
                    Memo = "포트폴리오 반응 좋았음. 기술면접 통과.",
                    StageLogs =
                    {
                        new StageLog { Stage = Stage.Applied,         ChangedAt = DateTime.Now.AddDays(-18) },
                        new StageLog { Stage = Stage.DocumentPassed,  ChangedAt = DateTime.Now.AddDays(-14) },
                        new StageLog { Stage = Stage.FirstInterview,  ChangedAt = DateTime.Now.AddDays(-7)  },
                        new StageLog { Stage = Stage.SecondInterview, ChangedAt = DateTime.Now.AddDays(-1)  },
                    }
                },
                new Application
                {
                    Company = "당근", Position = "백엔드 개발자",
                    EmploymentType = EmploymentType.FullTime, Channel = Channel.Headhunter,
                    Priority = Priority.High, Stage = Stage.DocumentPassed,
                    Location = "서울 서초", ExpectedSalary = "협의",
                    AddedDate = DateTime.Today.AddDays(-10), AppliedDate = DateTime.Today.AddDays(-9),
                    NextEventDate = DateTime.Today.AddDays(-1), NextEventLabel = "1차 면접 일정 조율 중",
                    Contact = "○○헤드헌터 010-1234-5678",
                    Memo = "헤드헌터 통해 제안받음.",
                    StageLogs =
                    {
                        new StageLog { Stage = Stage.Applied,        ChangedAt = DateTime.Now.AddDays(-9) },
                        new StageLog { Stage = Stage.DocumentPassed, ChangedAt = DateTime.Now.AddDays(-3) },
                    }
                },
                new Application
                {
                    Company = "네이버", Position = "데이터 엔지니어",
                    EmploymentType = EmploymentType.FullTime, Channel = Channel.JobPosting,
                    Priority = Priority.Medium, Stage = Stage.Applied,
                    Location = "분당", ExpectedSalary = "5,500만",
                    AddedDate = DateTime.Today.AddDays(-5), AppliedDate = DateTime.Today.AddDays(-4),
                    Deadline = DateTime.Today.AddDays(1),
                    Memo = "서류 결과 대기 중.",
                    StageLogs = { new StageLog { Stage = Stage.Applied, ChangedAt = DateTime.Now.AddDays(-4) } }
                },
                new Application
                {
                    Company = "라인", Position = "서버 개발자",
                    EmploymentType = EmploymentType.FullTime, Channel = Channel.Referral,
                    Priority = Priority.High, Stage = Stage.Interested,
                    Location = "서울", ExpectedSalary = "",
                    AddedDate = DateTime.Today.AddDays(-2),
                    Deadline = DateTime.Today.AddDays(4),
                    Contact = "지인 추천 - 김OO",
                    Memo = "추천받음. 자소서 준비 필요.",
                },
                new Application
                {
                    Company = "쿠팡", Position = "프론트엔드 개발자",
                    EmploymentType = EmploymentType.Contract, Channel = Channel.JobPosting,
                    Priority = Priority.Low, Stage = Stage.Rejected,
                    Location = "서울 송파", ExpectedSalary = "",
                    AddedDate = DateTime.Today.AddDays(-25), AppliedDate = DateTime.Today.AddDays(-24),
                    Memo = "서류 탈락.",
                    StageLogs =
                    {
                        new StageLog { Stage = Stage.Applied,   ChangedAt = DateTime.Now.AddDays(-24) },
                        new StageLog { Stage = Stage.Rejected,  ChangedAt = DateTime.Now.AddDays(-19) },
                    }
                },
                new Application
                {
                    Company = "배달의민족", Position = "안드로이드 개발자",
                    EmploymentType = EmploymentType.FullTime, Channel = Channel.InboundOffer,
                    Priority = Priority.Medium, Stage = Stage.Offer,
                    Location = "서울 송파", ExpectedSalary = "6,200만",
                    AddedDate = DateTime.Today.AddDays(-30), AppliedDate = DateTime.Today.AddDays(-28),
                    NextEventDate = DateTime.Today.AddDays(3), NextEventLabel = "처우 협의 미팅",
                    Memo = "최종 합격! 처우 협의 중.",
                    StageLogs =
                    {
                        new StageLog { Stage = Stage.Applied,         ChangedAt = DateTime.Now.AddDays(-28) },
                        new StageLog { Stage = Stage.DocumentPassed,  ChangedAt = DateTime.Now.AddDays(-24) },
                        new StageLog { Stage = Stage.FirstInterview,  ChangedAt = DateTime.Now.AddDays(-18) },
                        new StageLog { Stage = Stage.SecondInterview, ChangedAt = DateTime.Now.AddDays(-10) },
                        new StageLog { Stage = Stage.Offer,           ChangedAt = DateTime.Now.AddDays(-3)  },
                    }
                },
            };

            Applications.AddRange(seed);

            // 자소서 샘플 (작성 탭에서 복붙해 쓰는 용도)
            CoverLetters.AddRange(
                new CoverLetter
                {
                    Title = "토스 지원동기",
                    Question = "우리 회사에 지원한 이유와 입사 후 포부를 작성해 주세요.",
                    Content = "사용자 경험을 최우선으로 고민하는 토스의 제품 철학에 깊이 공감하여 지원하게 되었습니다. (여기에 내용을 채워 넣고, 지원 시 복사해서 사용하세요.)",
                    UpdatedAt = DateTime.Now.AddDays(-3)
                },
                new CoverLetter
                {
                    Title = "공통 - 성장과정",
                    Question = "성장 과정과 가치관을 기술하시오.",
                    Content = "(여러 회사에 공통으로 쓰는 자기소개서 문단을 여기에 저장해두면 편합니다.)",
                    UpdatedAt = DateTime.Now.AddDays(-10)
                }
            );

            // 자격증 샘플
            Certificates.AddRange(
                new Certificate { Name = "정보처리기사", Issuer = "한국산업인력공단", AcquiredDate = DateTime.Today.AddMonths(-8) },
                new Certificate { Name = "SQLD (SQL 개발자)", Issuer = "한국데이터산업진흥원", AcquiredDate = DateTime.Today.AddMonths(-5) },
                new Certificate { Name = "TOEIC", Issuer = "ETS / YBM", AcquiredDate = DateTime.Today.AddMonths(-3), Number = "880점" }
            );

            // 학력 샘플
            Educations.AddRange(
                new Education { School = "한국대학교", Major = "컴퓨터공학과", Period = "2020.03~2024.02", Note = "학점 3.8/4.5" }
            );

            // 대외활동 샘플
            Activities.AddRange(
                new Activity { Title = "교내 개발 동아리", Organization = "한국대학교", Period = "2021~2023", Description = "팀 프로젝트 5건 진행" },
                new Activity { Title = "IT 봉사활동", Organization = "지역 청소년센터", Period = "2022", Description = "코딩 교육 보조" }
            );

            // 경력 샘플
            Careers.AddRange(
                new Career { Company = "(주)테크스타트", Role = "백엔드 인턴", Period = "2023.07~2023.12", Description = "REST API 개발 보조" }
            );

            // 다음 일정 종류 기본값
            EventTypes.AddRange(
                new EventType { Name = "서류 마감" },
                new EventType { Name = "1차 면접" },
                new EventType { Name = "2차 면접" },
                new EventType { Name = "임원 면접" },
                new EventType { Name = "인적성" },
                new EventType { Name = "처우 협의" },
                new EventType { Name = "최종 발표" }
            );

            SaveChanges();
        }
    }
}
