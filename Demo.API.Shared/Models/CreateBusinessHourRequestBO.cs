namespace Demo.API.Shared.Models;

public record CreateBusinessHourRequestBO
(
    DayOfWeek DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime
);
