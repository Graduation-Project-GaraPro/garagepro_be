using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class DecimalExtensions
    {
        public static string ToVnd(this decimal amount)
        {
            return amount.ToString("C0", new CultureInfo("vi-VN"));
        }
    }
}
