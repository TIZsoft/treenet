namespace Tizsoft.Extensions
{
    public static class NumberExt
    {
        public static float ToPercentage(this int val)
        {
            return val / 100f;
        }

        public static float ToPercentage(this uint val)
        {
            return val / 100f;
        }
    }
}