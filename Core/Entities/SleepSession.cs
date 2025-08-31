using System;

namespace SleepBot.Core.Entities;

public class SleepSession
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public DateTime SleepTime { get; set; }
    public DateTime WakeTime { get; set; }
    public TimeSpan Duration { get; set; }

    public DateTime CreatedAt { get; set; }
}
