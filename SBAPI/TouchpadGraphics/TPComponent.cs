using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace RZSB.TouchpadGraphics {
    public abstract class TPComponent : IDisposable {
        public delegate void TPComponentEvent(TPComponent sender);
        
        public virtual TPComponent Parent {
            get;
            internal set;
        }

        public virtual Point Position {
            get {
                return Bounds.Location;
            }
            set {
                Rectangle r = Bounds;
                r.Location = value;
                Bounds = r;
            }
        }

        public virtual Rectangle Bounds {
            get;
            protected set;
        }
        public bool Enabled {
            get;
            private set;
        }

        public event TPComponentEvent OnDisable;
        public virtual void Disable() {
            Enabled = false;
            if (OnDisable != null) OnDisable(this);
        }

        public event TPComponentEvent OnEnable;
        public virtual void Enable() {
            Enabled = true;
            if (OnEnable != null) OnEnable(this);
        }

        public event SBAPI.XYGestureDelegate OnTap;
        internal virtual void Tapped(ushort xPos, ushort yPos){
            if (OnTap != null) OnTap(xPos, yPos);
        }

        public event SBAPI.IntXYGestureDelegate OnPress;
        internal virtual void Pressed(uint touches, ushort xPos, ushort yPos) {
            if (OnPress != null) OnPress(touches, xPos, yPos);
        }

        public event SBAPI.IntXYGestureDelegate OnRelease;
        internal virtual void Released(uint touches, ushort xPos, ushort yPos) {
             if (OnRelease != null) OnRelease(touches, xPos, yPos);
        }

        public event SBAPI.XYGestureDelegate OnFingerOver;
        internal virtual void FingerOver(ushort xPos, ushort yPos) {
            if (OnFingerOver != null) OnFingerOver(xPos, yPos);
        }

        //override for custom drawing
        internal abstract void Draw(ref Graphics g);

        //only override if a top-level component!
        protected virtual void RequestTotalRedraw() {
            if (Parent != null) {
                Parent.RequestTotalRedraw();
            }
        }

        public virtual void Dispose() {
            //NOP
        }

        //some static constants for all TPComponents to use
        public static Color DEFAULT_BACKGROUND_COLOR = Color.Black;
        public static Color DEFAULT_FOREGROUND_COLOR = Color.Green;

        public const string DEFAULT_FONT_NAME = "Arial";
        public const float DEFAULT_FONT_SIZE = 12f;
        public const FontStyle DEFAULT_FONT_STYLE = FontStyle.Regular;
        public static Font GetDefaultFont() { return new Font(DEFAULT_FONT_NAME, DEFAULT_FONT_SIZE, DEFAULT_FONT_STYLE); }

        
    }
}
