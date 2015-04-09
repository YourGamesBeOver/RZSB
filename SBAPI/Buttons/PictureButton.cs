using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZSB.Buttons {
    class PictureButton : Button{

        private int priv_key;
        public override int DKey {
            get {
                return priv_key;
            }
            set {
                priv_key = value;
                if(Enabled)Redraw();
            }
        }

        private string normalIcon, pressedIcon;

        public string NormalIcon {
            get {
                return normalIcon;
            }
            set {
                normalIcon = value;
                Redraw();
            }
        }

        public string PressedIcon {
            get {
                return pressedIcon;
            }
            set {
                pressedIcon = value;
                Redraw();
            }
        }

        public PictureButton(int key, string normalIcon, string pressedIcon) : base(key) {
            this.normalIcon = normalIcon;
            this.pressedIcon = pressedIcon;
            
            Enable();
        }

        public override void Enable() {
            base.Enable();
            SBAPI.OnDynamicKeyEvent += SBAPI_OnDynamicKeyEvent;
            Redraw();
        }

        public override void Disable() {
            base.Disable();
            SBAPI.ClearDisplay((SBDisplays)DKey);

            SBAPI.OnDynamicKeyEvent -= SBAPI_OnDynamicKeyEvent;
        }

        void SBAPI_OnDynamicKeyEvent(int key, bool down) {
            if (Enabled && key == DKey) {
                FireButtonEvent(down);
            }
        }

        internal override void Redraw() {
            SBAPI.SendImageToDK(DKey, true, pressedIcon);
            SBAPI.SendImageToDK(DKey, false, normalIcon);
        }

        public override void Dispose() {
            //NOP
        }
    }
}
