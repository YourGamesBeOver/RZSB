using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RZSB.Buttons;

namespace RZSB.WindowSystem {
    public class ButtonPage {
        private Button[] Buttons = new Button[SBAPI.DK_COUNT];

        private bool enabled;


        public ButtonPage() {

        }

        public void Enable() {
            for (int i = 0; i<Buttons.Length; i++){
                if (Buttons[i] != null) Buttons[i].Enable();
            }
            enabled = true;
        }

        public void Disable() {
            for (int i = 0; i < Buttons.Length; i++) {
                if (Buttons[i] != null) Buttons[i].Disable();
            }
            enabled = false;
        }

        public Button SetButton(int buttonNumber, Button b) {
            Button old = Buttons[buttonNumber -1];
            if (enabled && old!=null) old.Disable();
            b.DKey = buttonNumber;
            if (enabled && b!=null) b.Enable();
            Buttons[buttonNumber - 1] = b;
            return old;
        }

        public Button GetButton(int buttonNumber) {
            return Buttons[buttonNumber - 1];
        }

        public bool AttachDelegateToButton(int buttonNumber, Button.OnButtonEventDelegate del) {
            if (GetButton(buttonNumber) == null) return false;
            GetButton(buttonNumber).OnButtonEvent += del;
            return true;
        }

        public bool RemoveDelegateFromButton(int buttonNumber, Button.OnButtonEventDelegate del) {
            if (GetButton(buttonNumber) == null) return false;
            GetButton(buttonNumber).OnButtonEvent -= del;
            return true;
        }
    }
}
