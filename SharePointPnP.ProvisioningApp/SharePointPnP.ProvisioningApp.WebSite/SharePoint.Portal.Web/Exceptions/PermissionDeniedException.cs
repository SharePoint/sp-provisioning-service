using System;
using System.Runtime.Serialization;

namespace SharePoint.Portal.Web.Exceptions
{
    public class PermissionDeniedException : Exception
    {
        public string PermissionName { get; set; }

        public PermissionDeniedException()
            : base()
        { }

        public PermissionDeniedException(string message)
            : base(message)
        { }

        public PermissionDeniedException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public PermissionDeniedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
