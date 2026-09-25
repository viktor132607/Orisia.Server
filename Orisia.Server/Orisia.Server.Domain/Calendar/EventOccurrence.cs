namespace Orisia.Server.Domain.Calendar;

public sealed record EventOccurrence(DateTime StartAt, DateTime? EndAt);
