using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZSB.Buttons {
    public abstract class Button : IDisposable {
        //true for pressed, false for released, I guess?
        public delegate void OnButtonEventDelegate(bool newState);
        public event OnButtonEventDelegate OnButtonEvent;
        private int priv_key;
        public virtual int DKey {
            get {
                return priv_key;
            }
            set {
                priv_key = value;
                if(Enabled) Redraw();
            }
        }
        public bool Enabled {
            get;
            private set;
        }

        protected Button(int key) {
            priv_key = key;
        }

        protected void FireButtonEvent(bool newState) {
            if (OnButtonEvent != null) OnButtonEvent(newState);
        }

        public abstract void Dispose();

        //called to enable the button
        public virtual void Enable() {
            Enabled = true;
        }

        //called to disable the button
        public virtual void Disable() {
            Enabled = false;
            SBAPI.ClearDisplay((SBDisplays)DKey);
        }

        //called when the button needs to do a full redraw
        internal abstract void Redraw();
        
    }
}
