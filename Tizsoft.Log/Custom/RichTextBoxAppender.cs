using System;
using System.Drawing;
using System.Windows.Forms;
using log4net.Appender;
using log4net.Core;
using log4net.Util;

namespace Tizsoft.Log.Custom
{
    /// <summary>
    /// Description of RichTextBoxAppender.
    /// </summary>
    public class RichTextBoxAppender : AppenderSkeleton
    {
        #region Private Instance Fields

        RichTextBox _richTextBox;
        public string TextBoxName { get; set; }
        public string FormName { get; set; }
        private readonly LevelMapping _levelMapping = new LevelMapping();
        const int MaxTextLength = 100000;
        #endregion

        private delegate void UpdateControlDelegate(LoggingEvent loggingEvent);

        static Color StringToColor(string color)
        {
            switch (color.ToLower())
            {
                case "red":
                    return Color.Red;

                case "green":
                    return Color.Green;

                case "blue":
                    return Color.Blue;

                case "cyan":
                    return Color.Cyan;

                case "orange":
                    return Color.Orange;

                case "yellow":
                    return Color.Yellow;

                case "white":
                    return Color.White;

                case "black":
                    return Color.Black;

                default:
                    return Color.White;
            }
        }

        void ParseColorFormat(LevelTextStyle style, string msg)
        {
            if (string.IsNullOrEmpty(msg))
                return;

            msg = msg.TrimStart();
            string[] colorEndTags = { "</color>" };
            var colorStartTag = "<color=";
            var colorStartTagEnd = ">";

            var frontColor = style != null ? style.TextColor : Color.White;

            if (msg.StartsWith("WARN"))
                frontColor = Color.Yellow;
            else if (msg.StartsWith("ERROR"))
                frontColor = Color.Red;
            else if (msg.StartsWith("FATAL"))
                frontColor = Color.PaleVioletRed;
            else if (msg.StartsWith("INFO"))
                frontColor = Color.Gray;

            var subStrings = msg.Split(colorEndTags, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < subStrings.Length; ++i)
            {
                var colorStartIndex = subStrings[i].IndexOf(colorStartTag);

                if (colorStartIndex == -1)
                {
                    _richTextBox.SelectionColor = frontColor;
                    _richTextBox.AppendText(subStrings[i]);
                    continue;
                }

                var frontColorStr = subStrings[i].Substring(0, colorStartIndex);
                _richTextBox.SelectionColor = frontColor;
                _richTextBox.AppendText(frontColorStr);
                var colorEndIndex = subStrings[i].IndexOf(colorStartTagEnd, colorStartIndex);
                var colorStr =
                    subStrings[i].Substring(colorStartIndex, colorEndIndex - colorStartIndex)
                        .Replace(colorStartTag, string.Empty);
                var coloredStr = subStrings[i].Substring(colorEndIndex + 1);
                _richTextBox.SelectionColor = StringToColor(colorStr);

                //if (i != subStrings.Length - 1)
                //    _richTextBox.AppendText(coloredStr);
                //else
                //    _richTextBox.AppendText(coloredStr + Environment.NewLine);
                _richTextBox.AppendText(coloredStr);
            }
            _richTextBox.ScrollToCaret();    
        }

        private void UpdateControl(LoggingEvent loggingEvent)
        {
            // There may be performance issues if the buffer gets too long
            // So periodically clear the buffer
            if (_richTextBox.TextLength > MaxTextLength)
            {
                _richTextBox.Clear();
                _richTextBox.AppendText(string.Format("(Cleared log length max: {0})\n", MaxTextLength));
            }

            // look for a style mapping
            var selectedStyle = _levelMapping.Lookup(loggingEvent.Level) as LevelTextStyle;
            if (selectedStyle != null)
            {
                // set the colors of the text about to be appended
                _richTextBox.SelectionBackColor = selectedStyle.BackColor;
                _richTextBox.SelectionColor = selectedStyle.TextColor;

                // alter selection font as much as necessary
                // missing settings are replaced by the font settings on the control
                if (selectedStyle.Font != null)
                {
                    // set Font Family, size and styles
                    _richTextBox.SelectionFont = selectedStyle.Font;
                }
                else if (selectedStyle.PointSize > 0 && _richTextBox.Font.SizeInPoints != selectedStyle.PointSize)
                {
                    // use control's font family, set size and styles
                    var size = selectedStyle.PointSize > 0.0f ? selectedStyle.PointSize : _richTextBox.Font.SizeInPoints;
                    _richTextBox.SelectionFont = new Font(_richTextBox.Font.FontFamily.Name, size, selectedStyle.FontStyle);
                }
                else if (_richTextBox.Font.Style != selectedStyle.FontStyle)
                {
                    // use control's font family and size, set styles
                    _richTextBox.SelectionFont = new Font(_richTextBox.Font, selectedStyle.FontStyle);
                }
            }

            ParseColorFormat(selectedStyle, RenderLoggingEvent(loggingEvent));

            //_richTextBox.AppendText(RenderLoggingEvent(loggingEvent));
            //_richTextBox.ScrollToCaret();
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (null == _richTextBox)
            {
                if (String.IsNullOrEmpty(FormName) ||
                    String.IsNullOrEmpty(TextBoxName))
                {
                    return;
                }

                var form = Application.OpenForms[FormName];
                if (null == form)
                {
                    return;
                }

                _richTextBox = form.Controls[TextBoxName] as RichTextBox;
                if (null == _richTextBox)
                {
                    return;
                }

                form.FormClosing += (s, e) => _richTextBox = null;
            }

            if (_richTextBox.InvokeRequired)
            {
                _richTextBox.Invoke(
                    new UpdateControlDelegate(UpdateControl),
                    new object[] { loggingEvent });
            }
            else
            {
                UpdateControl(loggingEvent);
            }
        }

        public void AddMapping(LevelTextStyle mapping)
        {
            _levelMapping.Add(mapping);
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            _levelMapping.ActivateOptions();
        }

        protected override bool RequiresLayout { get { return true; } }
    }

    public class LevelTextStyle : LevelMappingEntry
    {
        private Color _textColor;
        private Color _backColor;
        private FontStyle _fontStyle = FontStyle.Regular;
        private float _pointSize;
        private bool _bold;
        private bool _italic;
        private readonly string _fontFamilyName = null;
        private Font _font;

        public bool Bold { get { return _bold; } set { _bold = value; } }
        public bool Italic { get { return _italic; } set { _italic = value; } }
        public float PointSize { get { return _pointSize; } set { _pointSize = value; } }

        /// <summary>
        /// Initialize the options for the object
        /// </summary>
        /// <remarks>Parse the properties</remarks>
        public override void ActivateOptions()
        {
            base.ActivateOptions();
            if (_bold)
                _fontStyle |= FontStyle.Bold;
            if (_italic)
                _fontStyle |= FontStyle.Italic;

            if (_fontFamilyName != null)
            {
                var size = _pointSize > 0.0f ? _pointSize : 8.25f;
                try
                {
                    _font = new Font(_fontFamilyName, size, _fontStyle);
                }
                catch (Exception)
                {
                    _font = new Font("Arial", 8.25f, FontStyle.Regular);
                }
            }
        }

        public Color TextColor { get { return _textColor; } set { _textColor = value; } }
        public Color BackColor { get { return _backColor; } set { _backColor = value; } }
        public FontStyle FontStyle { get { return _fontStyle; } set { _fontStyle = value; } }
        public Font Font { get { return _font; } set { _font = value; } }
    }
}
