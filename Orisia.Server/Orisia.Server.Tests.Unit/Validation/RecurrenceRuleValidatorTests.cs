using Orisia.Server.Core.Exceptions;
using Orisia.Server.Domain.Validation;
using Xunit;

namespace Orisia.Server.Tests.Unit.Validation;

public class RecurrenceRuleValidatorTests
{
    [Theory]
    [InlineData("FREQ=WEEKLY;BYDAY=MO,WE,FR", "FREQ=WEEKLY;BYDAY=MO,WE,FR")]
    [InlineData("RRULE:FREQ=DAILY;INTERVAL=2;COUNT=10", "FREQ=DAILY;INTERVAL=2;COUNT=10")]
    [InlineData("freq=monthly;bymonthday=1,-1", "FREQ=MONTHLY;BYMONTHDAY=1,-1")]
    public void NormalizeAndValidate_ShouldNormalizeValidRule(string input, string expected)
    {
        Assert.Equal(expected, RecurrenceRuleValidator.NormalizeAndValidate(input));
    }

    [Theory]
    [InlineData("FREQ=WEEKLY;BYDAY=XX")]
    [InlineData("FREQ=DAILY;INTERVAL=0")]
    [InlineData("FREQ=DAILY;COUNT=2;UNTIL=20261231")]
    [InlineData("FREQ=HOURLY")]
    [InlineData("FREQ=WEEKLY;UNKNOWN=1")]
    public void NormalizeAndValidate_ShouldRejectInvalidRule(string input)
    {
        Assert.Throws<AppException>(() => RecurrenceRuleValidator.NormalizeAndValidate(input));
    }
}
