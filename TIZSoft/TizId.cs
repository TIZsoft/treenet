using System;
using System.Globalization;

namespace Tizsoft
{
    public abstract class TizId
    {
        // 0000000001~999999999
        public const uint MinId = 1;
        public const uint MaxId = 999999999;
        
        protected uint IdCounter;

        public static string Format(uint id, string format = "")
        {
            return string.IsNullOrWhiteSpace(format)
                ? id.ToString(CultureInfo.InvariantCulture)
                : string.Format(format, id);
        }

        public abstract uint Next();
        public abstract uint Current();

        public void SetIdCounter(uint id)
        {
            IdCounter = id;
        }
    }

    public class TizIdIncrement : TizId
    {
        public TizIdIncrement(uint id = MinId)
        {
            SetIdCounter(id);
        }

        public override uint Next()
        {
            if (IdCounter > MaxId)
            {
                throw new ArgumentOutOfRangeException("IdCounter",
                    string.Format("The value is maximum({0}).", MaxId));
            }

            return IdCounter++;
        }

        public override uint Current()
        {
            return IdCounter - 1;
        }
    }

    public class TizIdDecrease : TizId
    {
        public TizIdDecrease(uint id = MaxId)
        {
            SetIdCounter(id);
        }

        public override uint Next()
        {
            if (IdCounter < MinId)
            {
                throw new ArgumentOutOfRangeException("IdCounter",
                    string.Format("The value is Minimum({0}).", MinId));
            }

            return IdCounter--;
        }

        public override uint Current()
        {
            return IdCounter + 1;
        }
    }
}
