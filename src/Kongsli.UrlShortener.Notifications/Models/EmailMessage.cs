namespace Kongsli.UrlShortener.Notifications.Models;

internal class EmailMessage
{
    private const string BINDING_OPERATION = "create";

    public EmailMessage(string subject, string message, string operation = BINDING_OPERATION, string? emailTo = null)
    {
        MetaData = new(emailTo, subject);
        Data = message;
        Operation = operation;
    }

    public string Data { get; }

    public MessageMetadata MetaData { get; }

    public string Operation {get;}

    public class MessageMetadata
    {
        public MessageMetadata(string? emailTo, string subject)
        {
            EmailTo = emailTo;
            Subject = subject;
        }

        public string? EmailTo { get; }
        public string Subject { get; }
    }
}