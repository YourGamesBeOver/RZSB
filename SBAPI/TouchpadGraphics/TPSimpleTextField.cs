using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace RZSB.TouchpadGraphics {
    public class TPSimpleTextField : TPComponent {

        private const int HIGHLIGHT_OFFSET = 1;
        private const int TEXT_OFFSET = 3;

        private static TPSimpleTextField selectedTextField = null;
        private static bool tabTransfer = false;

        public delegate void OnEnterDelegate(string text);
        public event OnEnterDelegate OnEnter;

        #region Font Fields
        public float FontSize {
            get {
                return TextFont.SizeInPoints;
            }
            set {
                Font oldFont = TextFont;
                float newsize = value;
                if (newsize < 0f) {
                    newsize = 1f;
                    AutoResizeFont = true;
                }
                TextFont = new Font(FontName, newsize, TextFontStyle);
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

        private bool priv_autoResize;
        public bool AutoResizeFont {
            get { return priv_autoResize; }
            set {
                priv_autoResize = value;
                if (!value) {
                    FontSize = DEFAULT_FONT_SIZE;
                } else {
                    RequestTotalRedraw();
                }
            }

        }

        private bool priv_passwordMode = false;
        public bool PasswordMode {
            get { return priv_passwordMode; }
            set {
                if (priv_passwordMode != value) {
                    priv_passwordMode = value;
                    RequestTotalRedraw();
                }
            }
        }

        public TPSimpleTextField NextTextField = null;

        private Rectangle TextBounds;
        private Rectangle HighlightBounds;

        public override Size Size {
            get {
                return base.Size;
            }
            set {
                base.Size = value;
                TextBounds = new Rectangle(TEXT_OFFSET, TEXT_OFFSET, value.Width - TEXT_OFFSET * 2, value.Height - TEXT_OFFSET * 2);
                HighlightBounds = new Rectangle(HIGHLIGHT_OFFSET, HIGHLIGHT_OFFSET, value.Width - HIGHLIGHT_OFFSET * 2, value.Height - HIGHLIGHT_OFFSET * 2);
            }
        }

        public TPSimpleTextField(Rectangle bounds, Color backgroundColor, Color textColor, Color borderColor, Color highlightColor, Font font, bool newlineAllowed) {
            NewlineAllowed = newlineAllowed;
            Size = bounds.Size;
            Position = bounds.Location;
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

        void TPSimpleTextField_OnTap(int xPos, int yPos) {
            SBAPI.KeyboardCaptured = true;
            selectedTextField = this;
        }

        void SBAPI_OnPressGesture(uint touchpoints, ushort xPos, ushort yPos) {
            SBAPI.KeyboardCaptured = false;
        }

        void SBAPI_OnKeyDown(SBAPI.VK key, IntPtr modifier) {
            if (selectedTextField != this || tabTransfer) return;
            if (key == SBAPI.VK.BACKSPACE && Text.Length > 0) {
                Text = Text.Substring(0, Text.Length - 1);
            } else if (key == SBAPI.VK.RETURN) {
                if (NewlineAllowed && MoreCharactersAllowed()) Text += "\n";
                if (OnEnter != null) OnEnter(Text);
            } else if (key == SBAPI.VK.TAB && NextTextField != null) {
                //Util.Utils.printf("Passing focus!");
                selectedTextField = NextTextField;
                RequestTotalRedraw();
                tabTransfer = true;
            }
        }

        void SBAPI_OnKeyTyped(char key, IntPtr modifier) {
            if (selectedTextField != this) return;
            if (!MoreCharactersAllowed()) return;
            if (!char.IsControl(key)) Text += key;
        }

        internal override void Draw(ref Graphics g) {
            if (AutoResizeFont) {
                priv_TextFont = Util.Utils.FindFont(g, Text, Size, priv_TextFont);
            }
            g.FillRectangle(backgroundBrush, ToRect(Size));
            g.DrawRectangle(borderPen, ToRect(Size));
            if (SBAPI.KeyboardCaptured && selectedTextField == this) {
                g.DrawRectangle(highlightPen, HighlightBounds);
                if (tabTransfer) tabTransfer = false;
            }
            string text = Text;
            if (PasswordMode) {
                StringBuilder b = new StringBuilder(Text.Length);
                for (int i = 0; i < Text.Length; i++) {
                    b.Append('*');
                }
                text = b.ToString();
            }
            g.DrawString(text, TextFont, textBrush, TextBounds);
        }

        protected bool MoreCharactersAllowed() {
            if (CharacterLimit < 0) return true;
            if (Text.Length < CharacterLimit) return true;
            return false;
        }

        protected override void DisposeManagedResources() {
            base.DisposeManagedResources();
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
