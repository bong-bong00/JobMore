namespace JobMore.Models
{
    /// <summary>다음 일정 종류(서류 마감, 1차 면접 등) 드롭다운 항목.</summary>
    public class EventType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
