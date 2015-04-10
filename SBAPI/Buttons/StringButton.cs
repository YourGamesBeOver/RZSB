using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using RZSB.Buttons.Drawing;

namespace RZSB.Buttons {
    public class StringButton : BufferButton {
        public StringButton(int key, string text, Color textColor, Color backgroundColor, string fontName = SimpleTextButtonDrawer.DEFAULT_FONT_NAME) : 
            base(key, new SimpleTextButtonDrawer(text, textColor, backgroundColor, fontName)){
        }

        public StringButton(int key, string text, Color textColor, string fontName = SimpleTextButtonDrawer.DEFAULT_FONT_NAME) :
            this(key, text, textColor, SimpleTextButtonDrawer.DEFAULT_BACKGROUND_COLOR, fontName) {

        }

        public StringButton(int key, string text) : this(key, text, SimpleTextButtonDrawer.DEFAULT_TEXT_COLOR){
            
        }

        public void SetFont(string font) {
            ((SimpleTextButtonDrawer)Drawer).setFont(font);
            Redraw();
        }
        public void SetTextColor(Color c) {
            ((SimpleTextButtonDrawer)Drawer).setTextColor(c);
            Redraw();
        }

        public void SetBackgroundColor(Color c) {
            ((SimpleTextButtonDrawer)Drawer).setBackgroundColor(c);
            Redraw();
        }

        public void SetText(string txt) {
            ((SimpleTextButtonDrawer)Drawer).setText(txt);
            Redraw();
        }
    }
}
