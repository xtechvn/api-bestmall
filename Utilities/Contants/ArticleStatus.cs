using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Contants
{
    public struct ArticleStatus
    {
        public const int PUBLISH = 0; // BÀI XUẤT BẢN
        public const int SAVE = 1; // BÀI LƯU TẠM
        public const int REMOVE = 2; // BÀI BỊ HẠ
    }
    public enum ArticleType
    {
        NORMAL = 0,
        VIDEO = 1
    }
}
