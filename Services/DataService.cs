using System.Collections.Generic;
using System.Linq;
using JobMore.Data;
using JobMore.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMore.Services
{
    /// <summary>
    /// ViewModel과 DB 사이의 단일 통로.
    /// 여러 화면(지원현황/기록/작성/설정)이 같은 데이터를 공유하도록 싱글톤으로 운용한다.
    /// </summary>
    public class DataService
    {
        // ── 싱글톤 ──
        private static DataService _instance;
        public static DataService Instance => _instance ??= new DataService();

        private readonly TrackerDbContext _db;

        private DataService()
        {
            _db = new TrackerDbContext();
            _db.Initialize();
        }

        // ── 지원(Application) ──
        public List<Application> GetAll()
            => _db.Applications
                  .Include(a => a.StageLogs)
                  .OrderByDescending(a => a.AddedDate)
                  .ToList();

        public void Add(Application app)
        {
            _db.Applications.Add(app);
            _db.SaveChanges();
        }

        public void Delete(Application app)
        {
            _db.Applications.Remove(app);
            _db.SaveChanges();
        }

        public void LogStage(Application app, Stage stage)
        {
            var log = new StageLog { ApplicationId = app.Id, Stage = stage };
            app.StageLogs.Add(log);
            _db.StageLogs.Add(log);
            _db.SaveChanges();
        }

        /// <summary>특정 단계의 날짜 기록(StageLog)을 모두 삭제.</summary>
        public void RemoveStageLogs(Application app, Stage stage)
        {
            var logs = app.StageLogs.Where(l => l.Stage == stage).ToList();
            foreach (var l in logs)
            {
                app.StageLogs.Remove(l);
                _db.StageLogs.Remove(l);
            }
            _db.SaveChanges();
        }

        // ── 회원(User) ──
        public User GetUser() => _db.Users.FirstOrDefault();
        public bool HasUser() => _db.Users.Any();

        public void AddUser(User user)
        {
            _db.Users.Add(user);
            _db.SaveChanges();
        }

        /// <summary>이메일·비밀번호가 맞으면 true (단일 사용자 로컬 인증).</summary>
        public bool ValidateLogin(string email, string password)
        {
            var u = _db.Users.FirstOrDefault();
            if (u == null) return false;
            return string.Equals(u.Email?.Trim(), email?.Trim(), StringComparison.OrdinalIgnoreCase)
                && u.Password == password;
        }

        /// <summary>회원 탈퇴 — 회원 정보와 그가 만든 자소서·자격증까지 모두 삭제.</summary>
        public void DeleteUserAndData()
        {
            _db.Certificates.RemoveRange(_db.Certificates);
            _db.CoverLetters.RemoveRange(_db.CoverLetters);
            _db.Users.RemoveRange(_db.Users);
            _db.SaveChanges();
        }

        // ── 자소서(CoverLetter) ──
        public List<CoverLetter> GetCoverLetters()
            => _db.CoverLetters.OrderByDescending(c => c.UpdatedAt).ToList();

        public void AddCoverLetter(CoverLetter c)
        {
            _db.CoverLetters.Add(c);
            _db.SaveChanges();
        }

        public void DeleteCoverLetter(CoverLetter c)
        {
            _db.CoverLetters.Remove(c);
            _db.SaveChanges();
        }

        // ── 자격증(Certificate) ──
        public List<Certificate> GetCertificates()
            => _db.Certificates.OrderByDescending(c => c.AcquiredDate).ToList();

        public void AddCertificate(Certificate c)
        {
            _db.Certificates.Add(c);
            _db.SaveChanges();
        }

        public void DeleteCertificate(Certificate c)
        {
            _db.Certificates.Remove(c);
            _db.SaveChanges();
        }

        // ── 학력(Education) ──
        public List<Education> GetEducations() => _db.Educations.ToList();
        public void AddEducation(Education e) { _db.Educations.Add(e); _db.SaveChanges(); }
        public void DeleteEducation(Education e) { _db.Educations.Remove(e); _db.SaveChanges(); }

        // ── 대외활동(Activity) ──
        public List<Activity> GetActivities() => _db.Activities.ToList();
        public void AddActivity(Activity a) { _db.Activities.Add(a); _db.SaveChanges(); }
        public void DeleteActivity(Activity a) { _db.Activities.Remove(a); _db.SaveChanges(); }

        // ── 경력(Career) ──
        public List<Career> GetCareers() => _db.Careers.ToList();
        public void AddCareer(Career c) { _db.Careers.Add(c); _db.SaveChanges(); }
        public void DeleteCareer(Career c) { _db.Careers.Remove(c); _db.SaveChanges(); }

        public System.Collections.Generic.List<DesiredJob> GetDesiredJobs() => _db.DesiredJobs.ToList();
        public void AddDesiredJob(DesiredJob d) { _db.DesiredJobs.Add(d); _db.SaveChanges(); }
        public void DeleteDesiredJob(DesiredJob d) { _db.DesiredJobs.Remove(d); _db.SaveChanges(); }

        public System.Collections.Generic.List<EventType> GetEventTypes() => _db.EventTypes.ToList();
        public EventType AddEventType(string name)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return null;
            var existing = _db.EventTypes.FirstOrDefault(e => e.Name == name);
            if (existing != null) return existing;
            var et = new EventType { Name = name };
            _db.EventTypes.Add(et);
            _db.SaveChanges();
            return et;
        }

        // ── 공통 저장 ──
        public void Save() => _db.SaveChanges();

        /// <summary>저장하지 않은 변경(추가/수정/삭제)이 메모리에 남아있는지.</summary>
        public bool HasPendingChanges() => _db.ChangeTracker.HasChanges();
    }
}
