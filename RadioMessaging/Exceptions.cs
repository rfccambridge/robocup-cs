using System;
using System.Collections.Generic;
using System.Text;
namespace RFC.RadioMessaging
{
    public class ConnectionRefusedException : ApplicationException
    {
        public ConnectionRefusedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
