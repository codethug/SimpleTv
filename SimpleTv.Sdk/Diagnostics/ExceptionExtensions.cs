using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Diagnostics
{
    public static class ExceptionExtensions
    {
        public static string AsDetailedString(this Exception e)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=============================================");
            sb.AppendExceptionDetails(e);
            sb.AppendLine("=============================================");
            return sb.ToString();
        }

        private static void AppendExceptionDetails(this StringBuilder sb, Exception e, bool isInnerException = false)
        {
            if (!isInnerException)
            {
                sb.Append("Unhandled Exception: ");
            }
            else 
            {
                sb.Append("Inner Exception: ");
            }
            sb.AppendFormat("{0}: {1}\r\n", e.GetType().Name, e.Message);
            sb.AppendExceptionProperties(e);
            sb.AppendLine("Stack Trace:");
            sb.AppendLine(e.StackTrace);

            if (e.InnerException != null)
            {
                sb.AppendExceptionDetails(e.InnerException, true);
            }
        }

        private static void AppendExceptionProperties(this StringBuilder sb, Exception e)
        {
            if (e is WebException)
            {
                var we = (WebException)e;
                sb.AppendLine("Status: " + we.Status);
            }
        }
    }
}
