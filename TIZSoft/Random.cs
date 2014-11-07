namespace Tizsoft
{
    public static class Random
    {
        static readonly object SyncRoot = new object();
        static System.Random _random = new System.Random();

        public static void Seed(int seed)
        {
            lock (SyncRoot)
            {
                _random = new System.Random(seed);
            }
        }
        
        /// <summary>
        ///     Returns a nonnegative random number.
        /// </summary>
        /// <returns></returns>
        public static int Range()
        {
            lock (SyncRoot)
            {
                return _random.Next();
            }
        }

        /// <summary>
        ///     Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Range(int max)
        {
            lock (SyncRoot)
            {
                return _random.Next(max);
            }
        }
        
        /// <summary>
        /// Returns a number within a specified range.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Range(int min, int max)
        {
            lock (SyncRoot)
            {
                return _random.Next(min, max);
            }
        }

        public static double Range01()
        {
            lock (SyncRoot)
            {
                return _random.NextDouble();
            }
        }
    }
}
