namespace SharePoint.Portal.Web.Middleware.PortalApiExceptionHandler
{
    public class ApiExceptionHandlerOptions
    {
        /// <summary>
        /// (Optional) Boolean indicating if the stack trace and inner exception should be included with the return data. By default
        /// it will be set to false if not in development environment.
        /// </summary>
        public bool? IncludeDebugInfo { get; set; }

        /// <summary>
        /// The path start used to filter the requests that the middleware will handle. Defaults to "/api".
        /// </summary>
        public string PathStart { get; set; } = "/api";
    }
}
