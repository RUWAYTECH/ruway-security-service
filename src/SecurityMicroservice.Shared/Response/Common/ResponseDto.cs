namespace SecurityMicroservice.Shared.Response.Common
{
    public class ResponseDto
    {
        private static readonly IEnumerable<ApplicationMessage> DefaultMessages = Enumerable.Empty<ApplicationMessage>();

        public ResponseDto(IEnumerable<ApplicationMessage> messages = null)
        {
            Messages = new List<ApplicationMessage>(messages ?? DefaultMessages);
        }

        public bool IsValid => Messages.All(m => m.MessageType != ApplicationMessageType.Error);

        public List<ApplicationMessage> Messages { get; set; }



        public static ResponseDto Create(ApplicationMessage message)
        {
            if (message == null)
                return Create();

            return Create(messages: new ApplicationMessage[] { message });
        }

        public static ResponseDto Create(IEnumerable<ApplicationMessage> messages = null)
        {
            return new ResponseDto(messages);
        }


        public static ResponseDto<T> Create<T>(ApplicationMessage message)
        {
            var data = default(T);
            if (message == null)
                return Create(data);

            return Create(data: data, messages: new ApplicationMessage[] { message });
        }

        public static ResponseDto<T> Create<T>(T data)
        {
            return Create(data: data, messages: null);
        }

        public static ResponseDto<T> Create<T>(T data = default, IEnumerable<ApplicationMessage> messages = null)
        {
            return new ResponseDto<T>(data, messages);
        }

        public static ResponseDto<T> Error<T>(string errorMessage)
        {
            var message = new ApplicationMessage() { Message = errorMessage, MessageType = ApplicationMessageType.Error };
            return Create<T>(message);
        }

        public static ResponseDto Error(string errorMessage)
        {
            var message = new ApplicationMessage() { Message = errorMessage, MessageType = ApplicationMessageType.Error };
            return Create(message);
        }
    }

    public class ResponseDto<T> : ResponseDto
    {
        public ResponseDto(T data = default, IEnumerable<ApplicationMessage> messages = null) :base(messages)
        {
            Data = data;
        }

        public T Data { get; set; }
    }
}
