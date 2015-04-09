using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RZSB.TouchpadGraphics {
    public class TPSimpleTextField : TPComponent {

        private const int HIGHLIGHT_OFFSET = 1;
        private const int TEXT_OFFSET = 3;

        private static TPSimpleTextField selectedTextField = null;

        #region Font Fields
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

        private Font priv_TextFont;
        public Font TextFont {
            get {
                return priv_TextFont;
            }
            set {
                priv_TextFont = value;
                RequestTotalRedraw();
            }
        }
        #endregion //Color Fields

        #region Color and Brush Fields
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
        public Color BorderColor {
            get { return borderPen.Color; }
            set {
                borderPen.Color = value;
                RequestTotalRedraw();
            }
        }
        public Color HighlightColor {
            get { return highlightPen.Color; }
            set {
                highlightPen.Color = value;
                RequestTotalRedraw();
            }
        }
        private SolidBrush backgroundBrush, textBrush;
        private Pen borderPen, highlightPen;

        #endregion //Color and Brush Fields

        public int CharacterLimit = -1;

        private string priv_text = "";
        public string Text {
            get {
                return priv_text;
            }
            set {
                priv_text = value;
                RequestTotalRedraw();
            }
        }

        public bool NewlineAllowed {
            get;
            protected set;
        }

        private Rectangle TextBounds;
        private Rectangle HighlightBounds;

        public override Rectangle Bounds {
            get {
                return base.Bounds;
            }
            protected set {
                base.Bounds = value;
                TextBounds = new Rectangle(value.X + TEXT_OFFSET, value.Y + TEXT_OFFSET, value.Width - TEXT_OFFSET * 2, value.Height - TEXT_OFFSET * 2);
                HighlightBounds = new Rectangle(value.X + HIGHLIGHT_OFFSET, value.Y + HIGHLIGHT_OFFSET, value.Width - HIGHLIGHT_OFFSET * 2, value.Height - HIGHLIGHT_OFFSET * 2);
            }
        }

        public TPSimpleTextField(Rectangle bounds, Color backgroundColor, Color textColor, Color borderColor, Color highlightColor, Font font, bool newlineAllowed) {
            NewlineAllowed = newlineAllowed;
            Bounds = bounds;
            priv_TextFont = font;
            backgroundBrush = new SolidBrush(backgroundColor);
            textBrush = new SolidBrush(textColor);
            borderPen = new Pen(borderColor);
            highlightPen = new Pen(highlightColor, 4f);

            SBAPI.OnKeyTyped += SBAPI_OnKeyTyped;
            SBAPI.OnKeyDown += SBAPI_OnKeyDown;
            SBAPI.OnPressGesture += SBAPI_OnPressGesture;

            OnTap += TPSimpleTextField_OnTap;
        }

        public TPSimpleTextField(Rectangle bounds, bool newLineAllowed)
            : this(bounds, Color.White, Color.Black, Color.Gray, Color.Green, GetDefaultFont(), newLineAllowed) {

        }

        void TPSimpleTextField_OnTap(ushort xPos, ushort yPos) {
            SBAPI.KeyboardCaptured = true;
            selectedTextField = this;
        }

        void SBAPI_OnPressGesture(uint touchpoints, ushort xPos, ushort yPos) {
            SBAPI.KeyboardCaptured = false;
        }

        void SBAPI_OnKeyDown(SBAPI.VK key, IntPtr modifier) {
            if (selectedTextField != this) return;
            if (key == SBAPI.VK.BACKSPACE && Text.Length > 0) {
                Text = Text.Substring(0, Text.Length - 1);
            } else if (key == SBAPI.VK.RETURN) {
                if (NewlineAllowed && MoreCharactersAllowed()) Text += "\n";
            }
        }

        void SBAPI_OnKeyTyped(char key, IntPtr modifier) {
            if (selectedTextField != this) return;
            if (MoreCharactersAllowed()) Text += key;
        }

        internal override void Draw(ref Graphics g) {
            g.FillRectangle(backgroundBrush, Bounds);
            g.DrawRectangle(borderPen, Bounds);
            if (SBAPI.KeyboardCaptured && selectedTextField == this) {
                g.DrawRectangle(highlightPen, HighlightBounds);
            }
            g.DrawString(Text, TextFont, textBrush, TextBounds);
        }

        protected bool MoreCharactersAllowed() {
            if (CharacterLimit < 0) return true;
            if (Text.Length < CharacterLimit) return true;
            return false;
        }

        public override void Dispose() {
            base.Dispose();
            backgroundBrush.Dispose();
            textBrush.Dispose();
            borderPen.Dispose();
            highlightPen.Dispose();
            SBAPI.OnKeyTyped -= SBAPI_OnKeyTyped;
            SBAPI.OnKeyDown -= SBAPI_OnKeyDown;
            SBAPI.OnPressGesture -= SBAPI_OnPressGesture;
        }
    }
}
