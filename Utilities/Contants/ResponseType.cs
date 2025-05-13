using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
    public enum ResponseType
    {
        SUCCESS = 0,
        FAILED = 1,
        ERROR = 2,
        EXISTS = 3,
        PROCESSING = 4,
        EMPTY = 5,
        CONFIRM = 6,
        NOT_EXISTS = 7
    }
    public class ResponseTypeString
    {
        public const string Success = "Success";
        public const string Fail = "Fail";
    }
}
