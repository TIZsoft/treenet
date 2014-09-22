namespace Tizsoft.Helpers
{
	public class Utils
	{
		public static bool IsBitSet(int number, int index)
		{
			if (index < 0 || index > sizeof (int))
				return false;

			return (number & (1 << index % 32)) != 0;
		}

		public static int SetBit(int number, int index, bool bitState)
		{
			if (index < 0 || index > sizeof(int))
				return number;

			return bitState ? number | (1 << index % 32) : number & ~(1 << index % 32);
		}
	}
}