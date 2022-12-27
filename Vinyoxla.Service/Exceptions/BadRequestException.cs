using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Service.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string msg) : base(msg)
        {

        }
    }
}
