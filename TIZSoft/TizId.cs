using System;
using System.Globalization;

namespace Tizsoft
{
    public abstract class TizId
    {
        // 0000000001~999999999
        public const uint MinId = 1;
        public const uint MaxId = 999999999;
        
        protected uint CurrentId;

        public static string Format(uint id, string format = "")
        {
            return string.IsNullOrWhiteSpace(format)
                ? id.ToString(CultureInfo.InvariantCulture)
                : string.Format(format, id);
        }

        public abstract uint Next();
        public abstract uint Current();

        public void SetCurrentId(uint id)
        {
            CurrentId = id;
        }
    }

    public class TizIdIncrement : TizId
    {
        public TizIdIncrement(uint current = MinId)
        {
            SetCurrentId(current);
        }

        public override uint Next()
        {
            if (CurrentId > MaxId)
            {
                throw new ArgumentOutOfRangeException("CurrentId",
                    string.Format("The value is maximum({0}).", MaxId));
            }

            return CurrentId++;
        }

        public override uint Current()
        {
            return CurrentId;
        }
    }

    public class TizIdDecrease : TizId
    {
        public TizIdDecrease(uint current = MaxId)
        {
            CurrentId = current;
        }

        public override uint Next()
        {
            if (CurrentId < MinId)
            {
                throw new ArgumentOutOfRangeException("CurrentId",
                    string.Format("The value is Minimum({0}).", MinId));
            }

            return CurrentId--;
        }

        public override uint Current()
        {
            return CurrentId;
        }
    }
}
