using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Service.Exceptions
{
    public class RecordDublicateException : Exception
    {
        public RecordDublicateException(string msg) : base(msg)
        {

        }
    }
}
