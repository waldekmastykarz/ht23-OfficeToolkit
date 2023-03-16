
namespace HttpSample.Common.Models
{
    public class FunctionCallerIdentity
    {
        public const string HEADER_CALLING_APPLICATION = "calling-application";

        public const string HEADER_CALLING_APPLICATION_OWNER = "calling-application-owner";

        public string CallerAppName { get; set; }
        public string CallerOwnerName { get; set; }

    }
}
