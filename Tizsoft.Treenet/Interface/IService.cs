using System;
using System.Threading.Tasks;

namespace Tizsoft.Treenet.Interface
{
    public interface IService
    {
        void Start();
        void Setup(EventArgs configArgs);
        void Update();
        Task UpdateAsync();
        void Stop();
        bool IsWorking { get; }
    }
}