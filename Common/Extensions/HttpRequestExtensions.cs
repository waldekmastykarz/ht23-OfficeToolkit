using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using HttpSample.Common.Models;

namespace HttpSample.Common.Extensions
{
    public static class HttpRequestExtensions
    {
        public static T GetHeader<T>(this HttpRequest req, ILogger logger, string correlationId, string headerName)
        {
            var headers = req.Headers;
            var headerIsMissing = !headers.TryGetValue(headerName, out var strValue);
            logger.LogInformation($"HTTP Header '{headerName}' value = '{strValue}'. Will try to cast value to type '{typeof(T).Name}'. Support correlationId={correlationId}");

            if (headerIsMissing)
            {
                throw new InvalidOperationException($"Expected HTTP Headers to contain '{headerName}' but it was not found");
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(strValue as dynamic);
            }

            var messageStr = $"Expected HTTP Headers '{headerName}' to be of type {typeof(T).Name}. Could not convert '{strValue}'";
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(strValue, out var boolValue))
                {
                    return boolValue as dynamic;
                }
                throw new InvalidOperationException(messageStr);
            }

            throw new InvalidOperationException($"Logic to convert HTTP Headers '{headerName}' with value '{strValue}' to type '{typeof(T).Name}' is missing");
        }

        public static bool TryGetHeader<T>(this HttpRequest req, ILogger logger, string correlationId, string headerName, out T value)
        {
            var headers = req.Headers;
            var headerIsMissing = !headers.TryGetValue(headerName, out var _);

            value = default;
            if (headerIsMissing)
            {
                logger.LogInformation($"HTTP Header '{headerName}' value was nt supplied, will return default value of '{typeof(T).Name}'. Support correlationId={correlationId}");
                return false;
            }

            value = GetHeader<T>(req, logger, correlationId, headerName);
            return true;
        }

        public static T GetQueryParam<T>(this HttpRequest req, ILogger logger, string correlationId, string queryParamName)
        {
            var queryParams = req.Query;
            var queryParamIsMissing = !queryParams.TryGetValue(queryParamName, out var strValue);
         
            logger.LogInformation($"HTTP Query Parm '{queryParamName}' value ='{strValue}'. Will try to cast value to type '{typeof(T).Name}'. Support CorrelationId={correlationId}");

            if (queryParamIsMissing)
            {
                throw new InvalidOperationException($"Expected HTTP Query Params to cotain '{queryParamName}' but it was missing");
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(strValue as dynamic);
            }

            var msgStr = $"Expected HTTP Query Parameters '{queryParamName}' to be type {typeof(T).Name}. Could not convert '{strValue}' to {typeof(T).Name}";
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(strValue, out var boolValue))
                {
                    return boolValue as dynamic;
                }
                throw new InvalidOperationException(msgStr);
            }

            throw new InvalidOperationException($"Logic to convert HTTP Query Param '{queryParamName}' with value '{strValue}' to type '{typeof(T).Name}' is missing");
        }

        public static bool TryGetQueryParam<T>(this HttpRequest req, ILogger logger, string correlationId, string queryParamName, out T value)
        {
            var queryParams = req.Query;
            var queryParamIsMissing = !queryParams.TryGetValue(queryParamName, out var _);

            value = default;
            if (queryParamIsMissing)
            {
                logger.LogInformation($"HTTP Query Param '{queryParamName}' value was not supplied will return default value of '{typeof(T).Name}'");
                return false;
            }

            value = req.GetQueryParam<T>(logger, correlationId, queryParamName);
            return true;
        }

        public static string GetFromRequestOrNewCorrelationId(this HttpRequest req, ILogger logger)
        {
            var requestTelemetry = req.HttpContext.Features.Get<RequestTelemetry>();
            var operationId = requestTelemetry.Context.Operation.Id;

            var correlationId = string.IsNullOrWhiteSpace(operationId) ? Guid.NewGuid().ToString() : operationId;
            var exists = TryGetHeader<string>(req, logger, correlationId, "correlation-id", out var correlationIdFromHeader);
            return exists ? correlationIdFromHeader : correlationId;
        }

        public static async Task<string> GetBodyAsync(this HttpRequest request)
        {
            var sr = new StreamReader(request.Body);
            var requestBody = await sr.ReadToEndAsync();

            // must reset the position of the reader to enable another read
            if (request.Body.CanSeek)
            {
                request.Body.Seek(0, SeekOrigin.Begin);
            }

            return requestBody;
        }

        public static async Task<T> GetBodyAsync<T>(this HttpRequest request, bool ignoreError = false)
        {
            var requestBody = await request.GetBodyAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return default;
            }

            var message = requestBody.FromJson<T>(NamingStrategyType.CamelCase, ignoreError);
            return message;
        }


        public static void ValidateHeaders(this HttpRequest req, ILogger logger, string correlationId)
        {
            var hasHeaderCallingApp = req.TryGetHeader(logger, correlationId, FunctionCallerIdentity.HEADER_CALLING_APPLICATION, out string callingApp);
            var hasHeaderCallingAppOwner = req.TryGetHeader(logger, correlationId, FunctionCallerIdentity.HEADER_CALLING_APPLICATION_OWNER, out string callingAppOwner);

            var headersContainsExpectedEntries = hasHeaderCallingApp && hasHeaderCallingAppOwner;
            if (!headersContainsExpectedEntries)
            {
                throw new InvalidOperationException($"Expected HTTP Header to contain header to headers. '{FunctionCallerIdentity.HEADER_CALLING_APPLICATION}' HTTP Headers need to be set!");
            }

            var headerValuesAreInvalid = string.IsNullOrWhiteSpace(callingApp) || string.IsNullOrWhiteSpace(callingAppOwner);
            if (headerValuesAreInvalid)
            {
                throw new InvalidOperationException($"Function was called {FunctionCallerIdentity.HEADER_CALLING_APPLICATION}={callingApp}, {FunctionCallerIdentity.HEADER_CALLING_APPLICATION_OWNER}={callingAppOwner}");
            }

            logger.LogInformation($"Function was called {FunctionCallerIdentity.HEADER_CALLING_APPLICATION}={callingApp}");
            // TODO: Validate Content-Type
        }

        public static FunctionCallerIdentity GetCallerIdentity(this HttpRequest request, ILogger logger, string correlationId)
        {
            var hasCallingAppHeader = request.TryGetHeader(logger, correlationId, FunctionCallerIdentity.HEADER_CALLING_APPLICATION, out string callingApp);
            var hasCallingAppOwnerHeader = request.TryGetHeader(logger, correlationId, FunctionCallerIdentity.HEADER_CALLING_APPLICATION_OWNER, out string callingAppOwner);

            if (!hasCallingAppHeader)
            {
                var hasCallingAppQueryParam = request.TryGetQueryParam(logger, correlationId, FunctionCallerIdentity.HEADER_CALLING_APPLICATION, out callingApp);

                // If the caller provided their identity via Query Params, manually  set the header to standardization
                if (hasCallingAppQueryParam)
                {
                    request.Headers.Append(FunctionCallerIdentity.HEADER_CALLING_APPLICATION, callingApp);
                }
            }

            if (hasCallingAppOwnerHeader)
            {
                return new FunctionCallerIdentity
                {
                    CallerAppName = callingApp,
                    CallerOwnerName = callingAppOwner
                };
            }

            var hasCallingAppOwnerQueryParam = request.TryGetQueryParam(logger, correlationId, FunctionCallerIdentity.HEADER_CALLING_APPLICATION_OWNER, out callingAppOwner);
            
            // If the caller provided their identity via Query params, manually set the header for standardization
            if (hasCallingAppOwnerQueryParam)
            {
                request.Headers.Append(FunctionCallerIdentity.HEADER_CALLING_APPLICATION_OWNER, callingAppOwner);
            }

            return new FunctionCallerIdentity
            {
                CallerAppName = callingApp,
                CallerOwnerName = callingAppOwner
            };
        }
    }
}
