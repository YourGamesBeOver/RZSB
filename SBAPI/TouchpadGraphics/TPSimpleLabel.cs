using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RZSB.TouchpadGraphics {
    public class TPSimpleLabel : TPComponent {
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

        public TPSimpleLabel(Point position, string text) {
            priv_text = text;
            priv_font = GetDefaultFont();
            textBrush = new SolidBrush(DEFAULT_FOREGROUND_COLOR);
            backgroundBrush = new SolidBrush(DEFAULT_BACKGROUND_COLOR);
            Bounds = new Rectangle(0, 0, 0, 0);
            Position = position;
            remeasure = true;
        }

        internal override void Draw(ref Graphics g) {
            if (remeasure) {
                Size size = g.MeasureString(Text, TextFont).ToSize();
                Rectangle bounds = Bounds;
                bounds.Width = size.Width;
                bounds.Height = size.Height;
                Bounds = bounds;
                remeasure = false;
            }
            g.FillRectangle(backgroundBrush, Bounds);
            g.DrawString(Text, TextFont, textBrush, Position);
        }

        public override void Dispose() {
            base.Dispose();
            textBrush.Dispose();
            TextFont.Dispose();
            backgroundBrush.Dispose();
        }
    }
}
