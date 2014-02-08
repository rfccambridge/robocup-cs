using System;
using System.Collections.Generic;
using System.Text;
namespace RFC.InterProcessMessaging
{
    public class ConnectionRefusedException : ApplicationException
    {
        public ConnectionRefusedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
