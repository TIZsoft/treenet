using System.Drawing;

namespace TestFormApp
{
    public class TizColorConst
    {
        public static Color HtmlRed { get { return Color.Red; } }
        public static Color HtmlGreen { get { return Color.Green; } }
        public static Color HtmlBlue { get { return Color.Blue; } }
        public static Color HtmlCyan { get { return Color.Cyan; } }
        public static Color HtmlOrange { get { return Color.Orange; } }
        public static Color HtmlYellow { get { return Color.Yellow; } }
        public static Color HtmlWhite { get { return Color.White; } }
        public static Color HtmlBlack { get { return Color.Black; } }
        public static Color HtmlGray { get { return Color.Gray; } }

        public static Color LogException { get { return HtmlRed; } }
        public static Color LogWarning { get { return HtmlYellow; } }
        public static Color LogError { get { return HtmlRed; } }
        public static Color LogNormal { get { return HtmlGray; } }

        public static Color HtmlServerOutput { get { return HtmlOrange; } }

        public static Color StringToColor(string color)
        {
            switch (color.ToLower())
            {
                case "red":
                    return HtmlRed;
                 
                case "green":
                    return HtmlGreen;

                case "blue":
                    return HtmlBlue;

                case "cyan":
                    return HtmlCyan;

                case "orange":
                    return HtmlOrange;

                case "yellow":
                    return HtmlYellow;

                case "white":
                    return HtmlWhite;

                case "black":
                    return HtmlBlack;

                default:
                    return HtmlWhite;
            }
        }
    }
}