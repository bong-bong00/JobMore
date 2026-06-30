using JobMore.Models;

namespace JobMore.ViewModels
{
    /// <summary>
    /// Application(엔티티)을 화면용으로 감싼 ViewModel.
    /// D-day, 진행/종료 여부 같은 계산값을 바인딩으로 노출한다.
    /// </summary>
    public class ApplicationViewModel : ViewModelBase
    {
        public Application Model { get; }

        public ApplicationViewModel(Application model) => Model = model;

        public int Id => Model.Id;

        public string Company
        {
            get => Model.Company;
            set { Model.Company = value; OnPropertyChanged(); }
        }

        public string Position
        {
            get => Model.Position;
            set { Model.Position = value; OnPropertyChanged(); }
        }

        public EmploymentType EmploymentType
        {
            get => Model.EmploymentType;
            set { Model.EmploymentType = value; OnPropertyChanged(); }
        }

        public Channel Channel
        {
            get => Model.Channel;
            set { Model.Channel = value; OnPropertyChanged(); }
        }

        public Priority Priority
        {
            get => Model.Priority;
            set { Model.Priority = value; OnPropertyChanged(); }
        }

        public Stage Stage
        {
            get => Model.Stage;
            set
            {
                if (Model.Stage == value) return;
                Model.Stage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsClosed));
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(IsInProgress));
            }
        }

        public string Location
        {
            get => Model.Location;
            set { Model.Location = value; OnPropertyChanged(); }
        }

        public string ExpectedSalary
        {
            get => Model.ExpectedSalary;
            set
            {
                // 숫자만 추출 → 쉼표 + '만' 자동 부착 (예: 5500 → 5,500만)
                var digits = new string((value ?? "").Where(char.IsDigit).ToArray());
                Model.ExpectedSalary = digits.Length == 0 ? "" : $"{long.Parse(digits):#,##0}만";
                OnPropertyChanged();
            }
        }

        public string JobUrl
        {
            get => Model.JobUrl;
            set { Model.JobUrl = value; OnPropertyChanged(); }
        }

        public string Contact
        {
            get => Model.Contact;
            set { Model.Contact = value; OnPropertyChanged(); }
        }

        public string Memo
        {
            get => Model.Memo;
            set { Model.Memo = value; OnPropertyChanged(); }
        }

        public string ColorHex
        {
            get => Model.ColorHex;
            set { Model.ColorHex = value; OnPropertyChanged(); }
        }

        public DateTime? AppliedDate
        {
            get => Model.AppliedDate;
            set { Model.AppliedDate = value; OnPropertyChanged(); }
        }

        public DateTime? Deadline
        {
            get => Model.Deadline;
            set { Model.Deadline = value; OnPropertyChanged(); RefreshDday(); }
        }

        public DateTime? NextEventDate
        {
            get => Model.NextEventDate;
            set { Model.NextEventDate = value; OnPropertyChanged(); RefreshDday(); }
        }

        public string NextEventLabel
        {
            get => Model.NextEventLabel;
            set { Model.NextEventLabel = value; OnPropertyChanged(); }
        }

        // ── 계산값 ──

        /// <summary>종료된 건(불합격/중도포기)인지</summary>
        public bool IsClosed => Stage is Stage.Rejected or Stage.Withdrawn;

        /// <summary>진행 중(관심~최종합격, 종료 아님)인지</summary>
        public bool IsActive => !IsClosed;

        /// <summary>홈(지원현황)에 보일 "진행중"인지 — 최종합격·불합격·중도포기는 제외(기록으로 이동).</summary>
        public bool IsInProgress => Stage is not (Stage.Offer or Stage.Rejected or Stage.Withdrawn);

        /// <summary>
        /// 가장 임박한 일정까지 남은 일수.
        /// 다음 일정(면접 등)을 우선, 없으면 서류 마감 기준. 둘 다 없으면 null.
        /// </summary>
        public int? Dday
        {
            get
            {
                DateTime? target = NextEventDate ?? Deadline;
                if (target == null || IsClosed) return null;
                return (target.Value.Date - DateTime.Today).Days;
            }
        }

        /// <summary>D-day 옆에 보여줄 일정 이름 (면접/마감)</summary>
        public string DdayLabel
        {
            get
            {
                if (IsClosed) return string.Empty;
                if (NextEventDate != null)
                    return string.IsNullOrWhiteSpace(NextEventLabel) ? "다음 일정" : NextEventLabel;
                if (Deadline != null) return "서류 마감";
                return string.Empty;
            }
        }

        public void RefreshDday()
        {
            OnPropertyChanged(nameof(Dday));
            OnPropertyChanged(nameof(DdayLabel));
        }
    }
}
