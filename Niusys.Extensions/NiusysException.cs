using System;
using System.Collections.Generic;
using System.Text;

namespace Niusys
{
    public class NiusysException : Exception
    {
        public NiusysException(string message) : base(message)
        {

        }
    }
}
