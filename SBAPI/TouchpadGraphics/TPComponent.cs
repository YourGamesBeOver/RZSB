using System;
using System.Drawing;

namespace RZSB.TouchpadGraphics {
    public abstract class TPComponent : IDisposable {
        private bool disposed = false;

        public delegate void TPComponentEvent(TPComponent sender);
        
        public virtual TPPanel Parent {
            get;
            internal set;
        }

        //position relative to the parent panel
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

        //position relative to the device
        public Point AbsolutePosition {
            get {
                if (Parent != null) return new Point(Parent.AbsolutePosition.X + Position.X, Parent.AbsolutePosition.Y + Position.Y);
                else return Position;
            }
            set {
                if (Parent != null) Position = new Point(value.X - Parent.AbsolutePosition.X, value.Y - Parent.AbsolutePosition.Y);
                else Position = value;
            }
        }

        public virtual Size Size {
            get {
                return Bounds.Size;
            }
            set {
                Rectangle r = Bounds;
                r.Size = value;
                Bounds = r;
            }
        }

        //bounds relative to the parent
        private Rectangle Bounds {
            get;
            set;
        }

        internal bool ContainsPoint(Point p) {
            return Bounds.Contains(p);
        }

        //bounds relative to the device
        public Rectangle AbsoluteBounds {
            get { 
                return new Rectangle(AbsolutePosition, Size); 
            }
            set {
                AbsolutePosition = value.Location;
                Size = value.Size;
            }
        }
        public bool Enabled {
            get;
            private set;
        }

        public event TPComponentEvent OnDisable;
        public virtual void Disable() {
            Enabled = false;
            if (OnDisable != null) OnDisable(this);
            RequestTotalRedraw();
        }

        public event TPComponentEvent OnEnable;
        public virtual void Enable() {
            Enabled = true;
            if (OnEnable != null) OnEnable(this);
            RequestTotalRedraw();
        }
        public delegate void TPComponentXYDelegate(int xPos, int yPos);

        public event TPComponentXYDelegate OnTap;
        internal virtual void Tapped(int xPos, int yPos){
            if (OnTap != null) OnTap(xPos, yPos);
        }

        public delegate void TPComponentUXYDelegate(uint touches, int xPos, int yPos);
        public event TPComponentUXYDelegate OnPress;
        internal virtual void Pressed(uint touches, int xPos, int yPos) {
            if (OnPress != null) OnPress(touches, xPos, yPos);
        }

        public event TPComponentUXYDelegate OnRelease;
        internal virtual void Released(uint touches, int xPos, int yPos) {
             if (OnRelease != null) OnRelease(touches, xPos, yPos);
        }

        public event TPComponentXYDelegate OnFingerOver;
        internal virtual void FingerOver(int xPos, int yPos) {
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

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TPComponent() {
            Dispose(false);
        }

        private void Dispose(bool disposing) {
            // Check to see if Dispose has already been called. 
            if (!this.disposed) {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing) {
                    // Dispose managed resources.
                    DisposeManagedResources();
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.
                DisposeUnmanagedResources();

                // Note disposing has been done.
                disposed = true;

            }
        }

        protected virtual void DisposeManagedResources() {

        }

        protected virtual void DisposeUnmanagedResources() {

        }

        //some static constants for all TPComponents to use
        public static Color DEFAULT_BACKGROUND_COLOR = Color.Black;
        public static Color DEFAULT_FOREGROUND_COLOR = Color.Green;

        public const string DEFAULT_FONT_NAME = "Arial";
        public const float DEFAULT_FONT_SIZE = 12f;
        public const FontStyle DEFAULT_FONT_STYLE = FontStyle.Regular;
        public static Font GetDefaultFont() { return new Font(DEFAULT_FONT_NAME, DEFAULT_FONT_SIZE, DEFAULT_FONT_STYLE); }

        protected static Rectangle ZeroPosition(Rectangle r) { return new Rectangle(0, 0, r.Width, r.Height); }
        protected static Rectangle ToRect(Size s) { return new Rectangle(Point.Empty, s); }
    }
}
