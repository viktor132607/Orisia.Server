using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.Common.Responses.Calendar;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class CalendarController(ICalendarService calendarService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CalendarResponse>> GetRange(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to)
    {
        return Ok(await calendarService.GetRangeAsync(from, to));
    }

    [HttpGet("month")]
    public async Task<ActionResult<CalendarResponse>> GetMonth(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        return Ok(await calendarService.GetMonthAsync(year, month));
    }

    [HttpGet("week")]
    public async Task<ActionResult<CalendarResponse>> GetWeek(
        [FromQuery] DateTimeOffset date)
    {
        return Ok(await calendarService.GetWeekAsync(date));
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IReadOnlyCollection<CalendarOccurrenceResponse>>> GetUpcoming(
        [FromQuery] int take = 5)
    {
        return Ok(await calendarService.GetUpcomingAsync(take));
    }
}
