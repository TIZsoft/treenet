using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tizsoft.Log;

namespace TestFormApp
{
    public class LogPrinter
    {
        private RichTextBox _richTextBox;
        private int _maxPrintCount;

        private void AddLogMsg(string msg)
        {
            if (string.IsNullOrEmpty(msg))
                return;

            string[] colorEndTags = new string[] { "</color>" };
            string colorStartTag = "<color=";
            string colorStartTagEnd = ">";

            Color mainColor = Color.Gray;

            if (msg.StartsWith("warning:"))
                mainColor = TizColorConst.LogWarning;
            else if (msg.StartsWith("error:"))
                mainColor = TizColorConst.LogError;
            else if (msg.StartsWith("exception:"))
                mainColor = TizColorConst.LogException;

            string[] subStrings = msg.Split(colorEndTags, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < subStrings.Length; ++i)
            {
                int colorStartIndex = subStrings[i].IndexOf(colorStartTag);

                if (colorStartIndex == -1)
                {
                    _richTextBox.SelectionColor = mainColor;
                    _richTextBox.AppendText(subStrings[i] + Environment.NewLine);
                    continue;
                }

                string mainColorStr = subStrings[i].Substring(0, colorStartIndex);
                _richTextBox.SelectionColor = mainColor;
                _richTextBox.AppendText(mainColorStr);
                int colorEndIndex = subStrings[i].IndexOf(colorStartTagEnd, colorStartIndex);
                string colorStr = subStrings[i].Substring(colorStartIndex, colorEndIndex - colorStartIndex).Replace(colorStartTag, string.Empty);
                string coloredStr = subStrings[i].Substring(colorEndIndex + 1);
                _richTextBox.SelectionColor = TizColorConst.StringToColor(colorStr);

                if (i != subStrings.Length - 1)
                    _richTextBox.AppendText(coloredStr);
                else
                    _richTextBox.AppendText(coloredStr + Environment.NewLine);
            }
            _richTextBox.ScrollToCaret();
        }

        public LogPrinter(RichTextBox richTextBox, int maxPrintCount = 500)
        {
            _richTextBox = richTextBox;
            _maxPrintCount = maxPrintCount;
        }
    }
}