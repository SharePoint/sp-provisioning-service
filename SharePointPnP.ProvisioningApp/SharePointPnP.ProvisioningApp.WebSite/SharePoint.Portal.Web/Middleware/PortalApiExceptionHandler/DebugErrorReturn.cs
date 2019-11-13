using System;

namespace SharePoint.Portal.Web.Middleware.PortalApiExceptionHandler
{
    public class DebugErrorReturn : ErrorReturn
    {
        public Exception Exception { get; set; }
    }
}
