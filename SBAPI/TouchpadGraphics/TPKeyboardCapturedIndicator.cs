using System;
using System.Text;
using System.Drawing;

namespace RZSB.TouchpadGraphics {
    public class TPKeyboardCapturedIndicator : TPPanel {
        private const int HEIGHT = 60;

        private static Rectangle staticBounds = new Rectangle(0, 0, SBAPI.TP_WIDTH, HEIGHT);
        private bool haveBeenDrawnYet = false;

        public override Point Position {
            get {
                return staticBounds.Location;
            }
        }

        public override Size Size {
            get {
                return staticBounds.Size;
            }
        }

        private TPSimpleLabel label;

        public TPKeyboardCapturedIndicator() {
            label = new TPSimpleLabel(new Point(SBAPI.TP_WIDTH / 2, 10), "Keyboard Active");
            label.DrawBackground = false;
            label.TextFontStyle = FontStyle.Bold;
            label.FontSize = 25f;
            label.TextColor = Color.Black;
            base.Add(label);

            this.BackgroundColor = Color.FromArgb(128, DEFAULT_FOREGROUND_COLOR);

            SBAPI.OnCaptureKeyboard += SBAPI_OnCaptureKeyboard;
            SBAPI.OnReleaseKeyboard += SBAPI_OnReleaseKeyboard;

            if (!SBAPI.KeyboardCaptured) Disable();
            else Enable();
        }

        internal override void Draw(ref Graphics g) {
            if (SBAPI.KeyboardCaptured) {
                base.Draw(ref g);
                if (!haveBeenDrawnYet) {
                    label.Draw(ref g);
                    Point temp = label.Position;
                    temp.X = (SBAPI.TP_WIDTH - label.Size.Width) / 2;
                    label.Position = temp;
                    haveBeenDrawnYet = true;
                    RequestTotalRedraw();
                }
            } else {
                Disable();
            }
        }

        void SBAPI_OnReleaseKeyboard() {
            Disable();
        }

        void SBAPI_OnCaptureKeyboard() {
            Enable();
        }

        public override void Add(TPComponent newChild) {
            //NOP
        }

        public override void Remove(TPComponent oldChild) {
            //NOP
        }


        protected override void DisposeManagedResources() {
            base.DisposeManagedResources();
            label.Dispose();
        }

    }
}
