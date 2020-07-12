using System;
using System.Collections.Generic;
using System.Text;

namespace CheckinLabs.BL.Models
{
    public sealed class UserAccountException : Exception
    {
        public UserAccountException(string msg) : base(msg)
        { }
    }
}
