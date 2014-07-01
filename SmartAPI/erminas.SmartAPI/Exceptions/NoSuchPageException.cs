using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.Exceptions
{
    public class NoSuchPageException : SmartAPIException
    {
        public NoSuchPageException(SmartAPIException e) : base(e.Server, "Could not load page", e)
        {
        }
    }
}
