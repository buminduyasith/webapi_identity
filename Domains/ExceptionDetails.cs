using System;

namespace webapi_identity.Domains
{
    public class ExceptionDetails
    {
        public string Message { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;

    }
}