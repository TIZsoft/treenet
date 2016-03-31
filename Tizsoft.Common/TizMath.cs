using System;
using System.Linq;

namespace Tizsoft
{
    public class TizMath
    {
        /// <summary>
        /// 真．四捨五入
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Round(double value)
        {
            return Math.Round(value, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 真．四捨五入
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal Round(decimal value)
        {
            return Math.Round(value, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 最大公因數
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Gcd(int a, int b)
        {
            return b == 0 ? a : Gcd(b, a % b);
        }

        /// <summary>
        /// 最小公倍數
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Lcm(int a, int b)
        {
            return a*b/Gcd(a, b);
        }
    }
}