namespace Orisia.Server.Common.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string DeliveryMode { get; set; } = "Console";

    public string SenderEmail { get; set; } = "noreply@orisia.local";

    public string SenderName { get; set; } = "Orisia";
    public string ResendApiKey { get; set; } = "";
    public string? InquiryRecipient { get; set; }

    public int PasswordResetTokenExpiryMinutes { get; set; } = 30;
}
