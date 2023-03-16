using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HttpSample.Common.Extensions
{
    public static class LoggerExtensions
    {
        public static ObjectResult LogAndGet500ServerErrorResponse(this ILogger logger, Exception ex, string logMessagePrefix, string correlationId)
        {
            logger.LogError($"{logMessagePrefix}. Support correlationId={correlationId}", ex);

            var result = new ObjectResult($"{logMessagePrefix}.\nError message:\n{ex.Message}.\nsee logs for more details. Support correlationId={correlationId}")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return result;
        }
    }
}
