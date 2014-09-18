using System.Drawing;

namespace TIZSoft
{
    public class TizColorConst
    {
        public static Color Red { get { return Color.Red; } }
        public static Color Green { get { return Color.Green; } }
        public static Color Blue { get { return Color.Blue; } }
        public static Color Cyan { get { return Color.Cyan; } }
        public static Color Orange { get { return Color.Orange; } }
        public static Color Yellow { get { return Color.Yellow; } }
        public static Color White { get { return Color.White; } }
        public static Color Black { get { return Color.Black; } }
        public static Color Gray { get { return Color.Gray; } }

        public static Color LogException { get { return Red; } }
        public static Color LogWarning { get { return Yellow; } }
        public static Color LogError { get { return Red; } }
        public static Color LogNormal { get { return Gray; } }

        public static Color ServerDefaul { get { return Orange; } }

        public static Color StringToColor(string color)
        {
            switch (color.ToLower())
            {
                case "red":
                    return Red;
                 
                case "green":
                    return Green;

                case "blue":
                    return Blue;

                case "cyan":
                    return Cyan;

                case "orange":
                    return Orange;

                case "yellow":
                    return Yellow;

                case "white":
                    return White;

                case "black":
                    return Black;

                default:
                    return White;
            }
        }
    }
}