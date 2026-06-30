namespace JobMore.Models
{
    /// <summary>전형 단계가 바뀐 시점 기록 (진행 이력 / 타임라인).</summary>
    public class StageLog
    {
        public int Id { get; set; }
        public Stage Stage { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.Now;

        public int ApplicationId { get; set; }
        public Application Application { get; set; }
    }
}
