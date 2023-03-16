using Azure.Storage.Queues.Models;
using HttpSample.Common.Models;

namespace HttpSample.Common.Extensions
{
    public static class QueueMessageExtensions
    {
        public static string GetMessageBody(this QueueMessage queueMessage)
        {
            var message = queueMessage.Body.ToString();
            return message;
        }

        public static T GetMessageBody<T>(this QueueMessage queueMessage, bool ignoreError = false)
        {
            var messageJson = queueMessage.Body.ToString();
            var message = messageJson.FromJson<T>(NamingStrategyType.CamelCaase, ignoreError);
            return message;
        }
    }
}
