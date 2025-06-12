using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class NumberHelpers
    {
        public static double RoundUpToHundredsDouble(double amount)
        {
            // Chia số tiền cho 100, làm tròn lên bằng Math.Ceiling, sau đó nhân lại với 100.
            return Math.Floor(amount / 500) * 500;
        }
    }
}
