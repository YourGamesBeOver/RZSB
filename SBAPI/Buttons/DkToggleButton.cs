using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace RZSB.Buttons {
    public class DkToggleButton<TOnButton, TOffButton> : Button  
        where TOnButton : Button 
        where TOffButton : Button {

        private TOnButton onButton;
        private TOffButton offButton;

        public override int DKey {
            get {
                return base.DKey;
            }
            set {
                base.DKey = value;
                onButton.DKey = value;
                offButton.DKey = value;
            }
        }

        private bool priv_state = true;
        public bool State {
            get {
                return priv_state;
            }
            set {
                if (priv_state != value) {
                    if (value) {
                        onButton.Disable();
                        offButton.Enable();
                    } else {
                        offButton.Disable();
                        onButton.Enable();
                    }
                    priv_state = value;
                }
            }
        }

        public DkToggleButton(int key, TOnButton onButton, TOffButton offButton) : base(key) {
            this.offButton = offButton;
            this.offButton.OnButtonEvent += offButton_OnButtonEvent;
            this.onButton = onButton;
            this.onButton.OnButtonEvent += onButton_OnButtonEvent;
            onButton.Disable();
            offButton.DKey = key;
            onButton.DKey = key;
        }

        public void Toggle() {
            State = !State;
        }

        internal override void Redraw() {
            if (State) {
                offButton.Redraw();
            } else {
                onButton.Redraw();
            }
        }

        public override void Dispose() {
            onButton.Dispose();
            offButton.Dispose();
        }

        void onButton_OnButtonEvent(bool newState) {
            if (!newState && onButton.Enabled) {
                State = true;
                FireButtonEvent(State);
            }
        }

        void offButton_OnButtonEvent(bool newState) {
            if (!newState && offButton.Enabled) {
                State = false;
                FireButtonEvent(State);
            }
        }




    }
}
