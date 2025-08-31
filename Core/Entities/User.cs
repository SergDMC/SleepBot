using System;

namespace SleepBot.Core.Entities
{
    public class User
    {
        public long ChatId { get; set; }
        public string? Username { get; set; }
        public int? TargetHours { get; set; }
        public DateTime CreatedAt { get; set; }
    }


}
