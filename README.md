# 잡모아 (JobMore)

> 흩어진 취업 지원 현황을 **한 곳에 모아서**, 합격은 **더(More)**.
> ("모아"=흩어진 정보를 모은다 + "More"=더 많은 합격/오퍼 — 이름에 두 의미)

취업·이직 지원을 추적하는 **WPF 데스크톱 앱**입니다. MVVM 패턴, 데이터 바인딩,
실제 DB(SQLite + EF Core)를 모두 사용하도록 만들어졌습니다.

---

## 실행 방법 (중요 — 그냥 F5)

1. Visual Studio 2022에서 `JobMore.csproj` (또는 솔루션)을 엽니다.
   - 설치 워크로드: **".NET 데스크톱 개발"**
2. **F5** (또는 Ctrl+F5). 처음 빌드 시 NuGet 패키지(EF Core Sqlite)가 자동 복원됩니다.
3. 첫 실행 → **회원가입** 화면이 뜹니다. 이름/이메일만 넣으면 시작됩니다.

> **DB는 따로 설치할 게 없습니다.** SQLite 파일(`jobtracker.db`)이
> 실행 폴더(`bin/Debug/net8.0-windows/`)에 자동 생성되고, 샘플 데이터가 들어갑니다.
> 처음부터 다시 보고 싶으면 그 `.db` 파일을 지우고 다시 실행하세요.

---

## 화면 구성 (사이드바 멀티뷰)

좌측 **다크 사이드바**로 4개 화면을 오갑니다. (2026 UX 트렌드 — calm UI + 명료한 위계 + 시그니처 색)

| 화면 | 하는 일 |
|------|---------|
| **지원현황** | 지원 목록·검색·단계 필터, 대시보드(전체/진행중/면접중/합격 + 서류·최종 합격률 퍼널), 상세 편집 |
| **기록** | 지원 이력(지원일 순) · 보유 자격증 · 작성한 자소서를 한눈에 모아 보기 |
| **작성** | 자소서를 미리 써두고 **복사하기**로 지원 사이트에 붙여넣기 · 자격증 **검색해서 담기** |
| **설정** | 회원 정보(이름·나이·이메일·연락처·희망직무) 수정 |

### 핵심 기능
- **단계 이동**: 상세 패널의 `← 이전 단계` / `다음 단계 →` 버튼. 이동할 때마다 진행 이력이 자동 기록됩니다.
  파이프라인: 관심 → 지원 → 서류합격 → 1차면접 → 2차면접 → 처우협의 → 최종합격 (불합격/중도포기 별도)
- **지원 사이트 바로가기**: 공고 URL을 저장해두면 `🔗 지원 사이트 바로가기` 버튼으로 원본 공고를 기본 브라우저에서 다시 엽니다.
- **자소서 복붙**: 작성 탭에서 자소서를 보관 → `📋 복사하기` → 지원 사이트에 Ctrl+V.
- **자격증 검색 담기**: 자격증을 직접 타이핑하지 않고, 검색창에 입력하면 마스터 목록(정보처리기사·SQLD·TOEIC 등 60여 개)에서 필터되어 `+ 담기`로 추가됩니다. (위치검색처럼)
- **D-day 색 경고**: 마감/면접일이 임박하면 빨강·주황·노랑으로 자동 강조.

---

## "원본 사이트에서 지원하면 자동으로 옮겨지나요?" → 아니요 (정직하게)

자동 연동은 **구조적으로 불가능**합니다. 발표 때 이 점을 정확히 말하는 게 좋습니다.
- 잡코리아·사람인·원티드는 "내가 어디 지원했는지"를 외부에 열어주는 **공개 API가 없습니다.**
- 자동 수집은 로그인 세션 탈취/크롤링이 필요 → **약관 위반**.
- 그래서 해외 트래커(Simplify·Teal)도 **수동 입력 + 브라우저 확장** 조합입니다. 100% 자동은 없습니다.

→ 잡모아의 가치는 "자동 연동"이 아니라 **"흩어진 걸 한 곳에 모아 D-day·합격률·자소서·자격증까지 관리 + 입력을 최대한 편하게(자소서 복붙·자격증 검색·URL 바로가기)"** 입니다.

---

## 차별점 (발표용 — 근거와 함께)

국내 취준생은 지원 현황을 대부분 **엑셀·노션 양식**으로 관리합니다. 근거:
- brunch·슈퍼루키 등에 공유되는 **취준일지 엑셀 양식**(기업명·서류제출기한·서류발표·필기/인적성·면접·최종발표·입사일 컬럼).
- **예스폼 "취업준비 정리 템플릿"**(5시트) — 이건 경쟁자가 아니라, 빈 정적 엑셀 파일을 판다는 것 자체가 **수요(빈틈)의 증거**.
- 노션 템플릿 후기 "엑셀보다 가시성 좋다".
- Recruiter Nation 2024: 채용 담당자 87%가 여전히 스프레드시트 사용.

> 해외에는 Simplify·Teal 같은 전용 트래커가 **있습니다**(정직하게 인정). 차별점은 이렇게 좁힙니다:
> **"엑셀·노션 양식은 정적 — 마감/면접 D-day 자동 색경고 + 단계별 합격률 퍼널 집계는 안 됨.
> 그걸 자동화한 한국어 데스크톱 전용 도구."**

---

## 과제 요구사항 매핑

| 요구 | 구현 |
|------|------|
| **MVVM** | `Models / ViewModels / Views` 분리. View엔 로직 없음(코드비하인드는 InitializeComponent만). 셸(`MainViewModel`)이 `CurrentViewModel`로 화면 전환 |
| **데이터 바인딩** | 모든 화면이 `{Binding}` 기반. `INotifyPropertyChanged`(`ViewModelBase`), `ICommand`(`RelayCommand`), `ObservableCollection`, `CollectionView` 필터, `IValueConverter`(단계 색/D-day/Enum 설명) |
| **DB** | **SQLite + EF Core 8 코드 퍼스트**. `TrackerDbContext`에 5개 테이블(Applications·StageLogs·Users·CoverLetters·Certificates), 1:N 관계 + Cascade 삭제, `EnsureCreated` 시드 |

> 과제 안내문이 **"MySQL"을 명시**한다면 감점 요인이 될 수 있습니다. 그 경우
> EF Core라 provider만 MySQL로 바꾸면 되지만, 서버 설치가 필요해집니다. 필요하면 말해 주세요.

---

## 프로젝트 구조

```
JobMore/
├─ Models/         Application, StageLog, Enums(단계 등), User, CoverLetter,
│                  Certificate, CertificateCatalog(자격증 검색 마스터 목록)
├─ Data/           TrackerDbContext (SQLite, 시드)
├─ Services/       DataService (싱글톤 — 모든 화면이 공유)
├─ ViewModels/     MainViewModel(셸), ApplicationsViewModel, RecordViewModel,
│                  WritingViewModel, SettingsViewModel, SignUpViewModel,
│                  ApplicationViewModel, RelayCommand, EnumHelper, ViewModelBase
├─ Views/          ApplicationsView, RecordView, WritingView, SettingsView, SignUpView
├─ Converters/     Enum설명 / 단계→색 / D-day→텍스트·색
├─ App.xaml        디자인 토큰(시그니처 그라데이션·카드·버튼·입력) + ViewModel→View 매핑
└─ MainWindow.xaml 다크 사이드바 셸 + 콘텐츠 영역
```

## 디자인 (2026 트렌드 반영)
- **다크 사이드바 + 밝은 본문 + 시그니처 색**(인디고 #4F46E5 → 바이올렛 #7C3AED 그라데이션)
- calm UI / 명료한 위계, 큰 헤드라인, 둥근 카드 + 부드러운 그림자
- 절제된 마이크로 인터랙션(버튼 호버 스케일, 입력 포커스 시 시그니처 테두리)
