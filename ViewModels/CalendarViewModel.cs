using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using JobMore.Models;
using JobMore.Services;

namespace JobMore.ViewModels
{
    /// <summary>일정 화면 — 월 달력에 마감·면접 일정을 표시.</summary>
    public class CalendarViewModel : ViewModelBase
    {
        private readonly DataService _data = DataService.Instance;
        private List<CalendarEvent> _allEvents = new();

        public ObservableCollection<CalendarDay> Days { get; } = new();
        public ObservableCollection<CalendarEvent> SelectedEvents { get; } = new();
        public string[] WeekHeaders { get; } = { "일", "월", "화", "수", "목", "금", "토" };

        public RelayCommand PrevMonthCommand { get; }
        public RelayCommand NextMonthCommand { get; }
        public RelayCommand TodayCommand { get; }
        public RelayCommand SelectDayCommand { get; }

        private DateTime _month;

        public CalendarViewModel()
        {
            LoadEvents();
            _month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            PrevMonthCommand = new RelayCommand(_ => { _month = _month.AddMonths(-1); Build(); });
            NextMonthCommand = new RelayCommand(_ => { _month = _month.AddMonths(1); Build(); });
            TodayCommand     = new RelayCommand(_ => { _month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); Build(); });
            SelectDayCommand = new RelayCommand(p => SelectDay(p as CalendarDay));

            Build();
            SelectDay(Days.FirstOrDefault(d => d.IsToday) ?? Days.FirstOrDefault(d => d.InMonth));
        }

        private void LoadEvents()
        {
            _allEvents = new List<CalendarEvent>();
            foreach (var a in _data.GetAll())
            {
                if (a.Deadline is DateTime)
                    _allEvents.Add(new CalendarEvent(a, true, "마감", "서류 마감", OnEventDateChanged));
                if (a.NextEventDate is DateTime)
                    _allEvents.Add(new CalendarEvent(a, false, "면접",
                        string.IsNullOrWhiteSpace(a.NextEventLabel) ? "면접/일정" : a.NextEventLabel,
                        OnEventDateChanged));
            }
        }

        /// <summary>일정 날짜가 바뀌면 저장 후 달력을 다시 그린다.</summary>
        private void OnEventDateChanged()
        {
            _data.Save();
            var keep = SelectedDay?.Date;
            LoadEvents();
            Build();
            var day = Days.FirstOrDefault(d => keep.HasValue && d.Date.Date == keep.Value.Date)
                      ?? Days.FirstOrDefault(d => d.IsToday)
                      ?? Days.FirstOrDefault(d => d.InMonth);
            SelectDay(day);
        }

        private string _monthLabel;
        public string MonthLabel { get => _monthLabel; private set => SetProperty(ref _monthLabel, value); }

        private int _monthDeadlineCount, _monthInterviewCount;
        public int MonthDeadlineCount { get => _monthDeadlineCount; private set => SetProperty(ref _monthDeadlineCount, value); }
        public int MonthInterviewCount { get => _monthInterviewCount; private set => SetProperty(ref _monthInterviewCount, value); }

        private void Build()
        {
            MonthLabel = $"{_month.Year}년 {_month.Month}월";
            Days.Clear();

            // 그 달 1일이 무슨 요일인지 → 앞쪽 빈칸(전월 일자)
            int startOffset = (int)_month.DayOfWeek; // 일=0
            DateTime first = _month.AddDays(-startOffset);

            for (int i = 0; i < 42; i++)
            {
                var date = first.AddDays(i);
                var evs = _allEvents.Where(e => e.Date == date.Date).ToList();
                Days.Add(new CalendarDay
                {
                    Date = date,
                    InMonth = date.Month == _month.Month,
                    IsToday = date.Date == DateTime.Today,
                    Events = evs
                });
            }

            MonthDeadlineCount = _allEvents.Count(e => e.Kind == "마감" && e.Date.Month == _month.Month && e.Date.Year == _month.Year);
            MonthInterviewCount = _allEvents.Count(e => e.Kind == "면접" && e.Date.Month == _month.Month && e.Date.Year == _month.Year);
        }

        private CalendarDay _selectedDay;
        public CalendarDay SelectedDay { get => _selectedDay; private set => SetProperty(ref _selectedDay, value); }

        private string _selectedDayLabel;
        public string SelectedDayLabel { get => _selectedDayLabel; private set => SetProperty(ref _selectedDayLabel, value); }

        public bool HasSelectedEvents => SelectedEvents.Count > 0;

        private void SelectDay(CalendarDay day)
        {
            if (SelectedDay != null) SelectedDay.IsSelected = false;
            SelectedDay = day;
            SelectedEvents.Clear();

            if (day == null)
            {
                SelectedDayLabel = "";
                OnPropertyChanged(nameof(HasSelectedEvents));
                return;
            }

            day.IsSelected = true;
            SelectedDayLabel = $"{day.Date.Month}월 {day.Date.Day}일 ({WeekHeaders[(int)day.Date.DayOfWeek]})";
            foreach (var e in day.Events.OrderBy(e => e.Kind)) SelectedEvents.Add(e);
            OnPropertyChanged(nameof(HasSelectedEvents));
        }
    }

    /// <summary>달력 한 칸.</summary>
    public class CalendarDay : ViewModelBase
    {
        public DateTime Date { get; set; }
        public int Day => Date.Day;
        public bool InMonth { get; set; }
        public bool IsToday { get; set; }
        public List<CalendarEvent> Events { get; set; } = new();

        public bool HasDeadline => Events.Any(e => e.Kind == "마감");
        public bool HasInterview => Events.Any(e => e.Kind == "면접");
        public bool HasAny => Events.Count > 0;

        private bool _selected;
        public bool IsSelected { get => _selected; set => SetProperty(ref _selected, value); }
    }

    /// <summary>달력에 표시할 일정 1건 — 원본 지원 건을 참조하여 날짜 수정 가능.</summary>
    public class CalendarEvent : ViewModelBase
    {
        private readonly Models.Application _app;
        private readonly bool _isDeadline;
        private readonly Action _onChanged;

        public CalendarEvent(Models.Application app, bool isDeadline, string kind, string label, Action onChanged)
        {
            _app = app; _isDeadline = isDeadline; _onChanged = onChanged;
            Kind = kind; Label = label;
        }

        public string Kind { get; }     // 마감 / 면접
        public string Label { get; }
        public string Company => _app.Company;

        // 날짜 편집 UI 표시 여부 (기본 숨김 → '날짜 변경' 버튼으로 펼침)
        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }

        /// <summary>캘린더 막대 색 — 회사 색이 지정돼 있으면 그 색, 아니면 종류별 기본색.</summary>
        public string BarColor =>
            string.IsNullOrWhiteSpace(_app.ColorHex)
                ? (Kind == "마감" ? "#E25C5C" : "#7C5CFC")
                : _app.ColorHex;

        public DateTime Date => (EditableDate ?? DateTime.Today).Date;

        /// <summary>달력/패널의 DatePicker가 바인딩 — 바꾸면 원본 지원 건의 날짜가 수정된다.</summary>
        public DateTime? EditableDate
        {
            get => _isDeadline ? _app.Deadline : _app.NextEventDate;
            set
            {
                if (_isDeadline) _app.Deadline = value;
                else _app.NextEventDate = value;
                OnPropertyChanged();
                _onChanged?.Invoke();
            }
        }
    }
}
