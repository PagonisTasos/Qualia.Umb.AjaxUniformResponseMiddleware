using System;

namespace Qualia.Umb.AjaxUniformResponseMiddleware
{
    public class UserFriendlyException : Exception
    {
        public string? UserFriendlyMessage { get; set; }
        public System.Net.HttpStatusCode StatusCode { get; set; }

        public UserFriendlyException(string? userFriendlyMessage, System.Net.HttpStatusCode StatusCode = System.Net.HttpStatusCode.InternalServerError) 
            : base()
        {
            UserFriendlyMessage = userFriendlyMessage;
            this.StatusCode = StatusCode;
        }
        public UserFriendlyException(string? message, string? userFriendlyMessage, System.Net.HttpStatusCode StatusCode = System.Net.HttpStatusCode.InternalServerError) 
            : base(message)
        {
            UserFriendlyMessage = userFriendlyMessage;
            this.StatusCode = StatusCode;
        }
        public UserFriendlyException(string? message, Exception? innerException, string? userFriendlyMessage, System.Net.HttpStatusCode StatusCode = System.Net.HttpStatusCode.InternalServerError) 
            : base(message, innerException)
        {
            UserFriendlyMessage = userFriendlyMessage;
            this.StatusCode = StatusCode;
        }
    }
}
