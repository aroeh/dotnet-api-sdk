namespace Demo.API.Shared.Models;

public record UpdateBusinessHourRequestBO
(
    DayOfWeek? DayOfWeek,
    TimeOnly? OpenTime,
    TimeOnly? CloseTime
);
