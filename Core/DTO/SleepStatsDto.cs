using System;

namespace SleepBot.Core.DTO;

public class SleepStatsDto
{
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan ShortestDuration { get; set; }
    public TimeSpan LongestDuration { get; set; }
}
