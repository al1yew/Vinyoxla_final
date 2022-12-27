using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Service.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string msg) : base(msg)
        {

        }
    }
}
