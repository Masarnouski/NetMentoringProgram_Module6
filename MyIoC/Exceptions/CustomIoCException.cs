using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIoC.Exceptions
{
    public class CustomIoCExpection : Exception
    {
        public CustomIoCExpection() : base()
        { }

        public CustomIoCExpection(string message) : base(message)
        { }
    }
}
