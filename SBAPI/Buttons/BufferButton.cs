using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using RZSB.Buttons.Drawing;

namespace RZSB.Buttons {
    public class BufferButton : Button{

        public Drawing.ButtonDrawer Drawer;

        private Bitmap bmp;

        public BufferButton(int key, Drawing.ButtonDrawer drawer) : base(key) {
            bmp = SBAPI.GenerateBitmapForDK();
            Drawer = drawer;

            Enable();
        }

        public override void Dispose() {
            SBAPI.OnDynamicKeyEvent -= SBAPI_OnDynamicKeyEvent;
            bmp.Dispose();
            Drawer.Dispose();
        }

        internal override void Redraw() {
            Redraw(false);
        }

        private void Redraw(bool pressed) {
            if (pressed) {
                Drawer.DrawPressed(ref bmp);
            } else {
                Drawer.DrawNormal(ref bmp);
            }

            SBAPI.WriteBitmapImageToSB((SBDisplays)DKey, bmp);
        }

        public override void Enable() {
            base.Enable();
            SBAPI.OnDynamicKeyEvent += SBAPI_OnDynamicKeyEvent;
            Redraw(false);
        }

        void SBAPI_OnDynamicKeyEvent(int key, bool down) {
            if (Enabled && key==DKey) {
                Redraw(down);
                FireButtonEvent(down);
            }
        }

        public override void Disable() {
            base.Disable();
            SBAPI.ClearDisplay((SBDisplays)DKey);
            
            SBAPI.OnDynamicKeyEvent -= SBAPI_OnDynamicKeyEvent;
        }
    }
}
