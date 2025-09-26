
namespace SecurityMicroservice.Shared.Response.Common
{
    public class ApplicationMessage
    {
        public ApplicationMessage()
        {
        }
        public ApplicationMessage(string key, string message, ApplicationMessageType messageType = default)
        {
            Key = key;
            Message = message;
            MessageType = messageType;
        }

        public string Key { get; set; }

        public string Message { get; set; }

        public ApplicationMessageType MessageType { get; set; }

        /// <inheritdoc />
        public override string ToString() => Message;
    }
}
