using System;

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
    }
}