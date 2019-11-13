using System;
using System.Net;
using System.Runtime.Serialization;

namespace SharePoint.Portal.Web.Exceptions
{
    [Serializable]
    public class PortalApiException : Exception
    {
        /// <summary>
        /// An error code that the API caller can use to quickly check against, if needed
        /// </summary>
        public string ApiErrorCode { get; set; } = "GeneralApiError";

        /// <summary>
        /// The http status code that should be returned to the APi caller from the server
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.InternalServerError;

        public PortalApiException()
        {
        }

        public PortalApiException(string message) : base(message)
        {
        }

        public PortalApiException(string message, string apiErrorCode) : base(message)
        {
            ApiErrorCode = apiErrorCode;
        }

        public PortalApiException(string message, HttpStatusCode httpStatusCode) : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }

        public PortalApiException(string message, HttpStatusCode httpStatusCode, string apiErrorCode) : base(message)
        {
            HttpStatusCode = httpStatusCode;
            ApiErrorCode = apiErrorCode;
        }


        public PortalApiException(string message, Exception innerException, HttpStatusCode httpStatusCode) : base(message, innerException)
        {
            HttpStatusCode = httpStatusCode;
        }

        public PortalApiException(string message, Exception innerException, HttpStatusCode httpStatusCode, string apiErrorCode) : base(message, innerException)
        {
            HttpStatusCode = httpStatusCode;
            ApiErrorCode = apiErrorCode;
        }

        protected PortalApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ApiErrorCode = info.GetString(nameof(ApiErrorCode));
            HttpStatusCode = (HttpStatusCode)info.GetInt32(nameof(HttpStatusCode));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            base.GetObjectData(info, context);
            info.AddValue(nameof(HttpStatusCode), HttpStatusCode);
            info.AddValue(nameof(ApiErrorCode), ApiErrorCode);
        }
    }
}
