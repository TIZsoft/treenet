namespace Tizsoft.Treenet.Interface
{
	public interface IService
	{
		void Start();
		void Stop();
		bool IsWorking { get; }
	}
}