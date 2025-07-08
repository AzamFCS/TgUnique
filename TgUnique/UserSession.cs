
namespace TgUnique
{
    public class UserSession
    {
        public long UserId {  get; set; }
        public IState CurrentState { get; set; }
        public DateTime? TrialStartedAt { get; set; }
        public bool IsTrial => TrialStartedAt != null && DateTime.UtcNow < TrialStartedAt.Value.AddDays(14);
        public List<YouTubeAcc> channels { get; set; }
        public int JsonAttempts { get; set; } = 0;
        public int DeletingAttempts { get; set; } = 0;
        public int VideoAttempts { get; set; } = 0;
    }
}
