using System.Collections.Concurrent;
using System.Text;

namespace Tizsoft.Database
{
    public class StringBuilderPool
    {
        readonly ConcurrentStack<StringBuilder> _builders = new ConcurrentStack<StringBuilder>();

        public StringBuilder GetBuilder()
        {
            StringBuilder builder = null;
            return _builders.TryPop(out builder) ? builder : new StringBuilder();
        }

        public void ReturnBuilder(StringBuilder builder)
        {
            if (builder != null)
            {
                builder.Remove(0, builder.Length);
                _builders.Push(builder);
            }
        }
    }
}