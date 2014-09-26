using System;

namespace Tizsoft.Treenet.Interface
{
	public interface IService
	{
		void Start();
	    void Setup(EventArgs configArgs);
		void Stop();
		bool IsWorking { get; }
	}
}