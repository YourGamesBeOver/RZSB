using System;
using System.Drawing;

namespace RZSB.TouchpadGraphics {
    public class TPSimpleLabel : TPComponent {
        const int DEFAULT_VERTICAL_PADDING = 4;
        const int DEFAULT_HORIZONTAL_PADDING = 7;

        private int priv_vertPad;
        public int VerticalPadding {
            get { return priv_vertPad; }
            set {
                priv_vertPad = value;
                remeasure = true;
                RequestTotalRedraw();
            }
        }
        private int priv_horizPad;
        public int HorizontalPadding {
            get { return priv_horizPad; }
            set {
                priv_horizPad = value;
                remeasure = true;
                RequestTotalRedraw();
            }
        }

        private bool remeasure = true;

        private String priv_text = "";
        public String Text {
            get { return priv_text; }
            set {
                priv_text = value;
                remeasure = true;
                RequestTotalRedraw();
            }
        }

        private SolidBrush backgroundBrush, textBrush;
        public Color BackgroundColor {
            get { return backgroundBrush.Color; }
            set {
                backgroundBrush.Color = value;
                RequestTotalRedraw();
            }
        }
        public Color TextColor {
            get { return textBrush.Color; }
            set {
                textBrush.Color = value;
                RequestTotalRedraw();
            }
        }

        public float FontSize {
            get {
                return TextFont.SizeInPoints;
            }
            set {
                Font oldFont = TextFont;
                TextFont = new Font(FontName, value, TextFontStyle);
                if (oldFont != null) oldFont.Dispose();
            }
        }
        public string FontName {
            get {
                return TextFont.FontFamily.Name;
            }
            set {
                Font oldFont = TextFont;
                TextFont = new Font(value, FontSize, TextFontStyle);
                if (oldFont != null) oldFont.Dispose();
            }
        }
        public FontStyle TextFontStyle {
            get {
                return TextFont.Style;
            }
            set {
                Font oldFont = TextFont;
                TextFont = new Font(FontName, FontSize, value);
                if (oldFont != null) oldFont.Dispose();
            }
        }

        private Font priv_font;
        public Font TextFont {
            get { return priv_font; }
            set {
                priv_font = value;
                RequestTotalRedraw();
                remeasure = true;
            }
        }

        private bool priv_drawBackground = false;
        public bool DrawBackground {
            get { return priv_drawBackground; }
            set {
                priv_drawBackground = value;
                RequestTotalRedraw();
            }
        }

        //private TextAlignment priv_alignment = TextAlignment.DEFAULT;
        //public TextAlignment Alignment {
        //    get { return priv_alignment; }
        //    set {
        //        priv_alignment = value;
        //        RequestTotalRedraw();
        //    }
        //}

        public TPSimpleLabel(Point position, string text) {
            priv_text = text;
            priv_font = GetDefaultFont();
            textBrush = new SolidBrush(DEFAULT_FOREGROUND_COLOR);
            backgroundBrush = new SolidBrush(DEFAULT_BACKGROUND_COLOR);
            Position = position;
            Size = Size.Empty;
            VerticalPadding = DEFAULT_VERTICAL_PADDING;
            HorizontalPadding = DEFAULT_HORIZONTAL_PADDING;
            remeasure = true;
        }

        internal override void Draw(ref Graphics g) {
            if (remeasure) {
                Size s = g.MeasureString(Text, TextFont).ToSize();
                s.Width += 2 * HorizontalPadding;
                s.Height += 2 * VerticalPadding;
                Size = s;
                remeasure = false;
            }
            if(DrawBackground) g.FillRectangle(backgroundBrush, ToRect(Size));
            g.DrawString(Text, TextFont, textBrush, new Point(HorizontalPadding, VerticalPadding));
        }

        protected override void DisposeManagedResources() {
            base.DisposeManagedResources();
            textBrush.Dispose();
            TextFont.Dispose();
            backgroundBrush.Dispose();
        }
    }
}
