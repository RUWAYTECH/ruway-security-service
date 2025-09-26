using System.Linq;

namespace SecurityMicroservice.Shared.Response.Common
{
    public static class ResponseDtoExtensions
    {
        public static T WithMessage<T>(this T response, string message, string key = null, ApplicationMessageType messageType = default) where T : ResponseDto
        {
            response.Messages.Add(new ApplicationMessage(key, message, messageType));
            return response;
        }

        public static T WithMessages<T>(this T response, params ApplicationMessage[] messages) where T : ResponseDto
        {
            response.Messages = messages.ToList();
            return response;
        }
    }
}
