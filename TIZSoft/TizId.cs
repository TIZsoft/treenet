using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tizsoft
{
    public interface ITizId
    {
        void Init(uint current = 0);
        uint Next();
        uint Current();
    }

    public abstract class TizId
    {
        // 0000000001~999999999
        protected const uint MinId = 1;
        protected const uint MaxId = 999999999;
        
        protected uint _current;

        public static string StringId(uint id)
        {
            return string.Format("{0:000000000}", id);
        }
    }

    public class TizIdForward : TizId, ITizId
    {
        public TizIdForward(uint current = 0)
        {
            Init(current);
        }

        public void Init(uint current = 0)
        {
            _current = current;
        }

        public uint Next()
        {
            if (_current == MaxId)
            {
                throw new ArgumentOutOfRangeException("current",
                    string.Format("The value is maximum({0}).", MaxId));
            }

            return ++_current;
        }

        public uint Current()
        {
            return _current;
        }
    }

    public class TizIdReverse : TizId, ITizId
    {
        public TizIdReverse(uint current = 0)
        {
            Init(current);
        }

        public void Init(uint current = 0)
        {
            _current = current;
        }

        public uint Next()
        {
            if (_current == MinId)
            {
                throw new ArgumentOutOfRangeException("current",
                    string.Format("The value is Minimum({0}).", MinId));
            }

            return --_current;
        }

        public uint Current()
        {
            return _current;
        }
    }
}
