using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RZSB.TouchpadGraphics {
    public class TPPanel : TPComponent{
        protected List<TPComponent> children = new List<TPComponent>();

        public Color BackgroundColor {
            get { return BackgroundBrush.Color; }
            set {
                BackgroundBrush.Dispose();
                BackgroundBrush = new SolidBrush(value);
            }
        }
        private SolidBrush BackgroundBrush = new SolidBrush(DEFAULT_BACKGROUND_COLOR);

        public override void Disable() {
            foreach (TPComponent c in children) c.Disable();
            base.Disable();
        }

        public override void Enable() {
            foreach (TPComponent c in children) c.Enable();
            base.Enable();
        }

        public virtual void Add(TPComponent newChild) {
            if (!children.Contains(newChild)) {
                children.Add(newChild);
                newChild.Parent = this;
                if (!Enabled && newChild.Enabled) {
                    newChild.Disable();
                }
            }
        }

        public virtual void Remove(TPComponent oldChild) {
            children.Remove(oldChild);
            oldChild.Parent = null;
        }

        public TPComponent GetChildAtPosition(Point p) {
            foreach (TPComponent c in children) {
                if (c.Bounds.Contains(p)) return c;
            }
            return null;
        }

        internal override void Tapped(ushort xPos, ushort yPos) {
            TPComponent child = GetChildAtPosition(new Point(xPos, yPos));
            if (child != null) {
                child.Tapped(xPos, yPos);
            } else {
                base.Tapped(xPos, yPos);
            }
        }

        internal override void Pressed(uint touches, ushort xPos, ushort yPos) {
            TPComponent child = GetChildAtPosition(new Point(xPos, yPos));
            if (child != null) {
                child.Pressed(touches, xPos, yPos);
            } else {
                base.Pressed(touches, xPos, yPos);
            }
        }

        internal override void Released(uint touches, ushort xPos, ushort yPos) {
            TPComponent child = GetChildAtPosition(new Point(xPos, yPos));
            if (child != null) {
                child.Released(touches, xPos, yPos);
            } else {
                base.Released(touches, xPos, yPos);
            }
        }

        internal override void FingerOver(ushort xPos, ushort yPos) {
            TPComponent child = GetChildAtPosition(new Point(xPos, yPos));
            if (child != null) {
                child.FingerOver(xPos, yPos);
            } else {
                base.FingerOver(xPos, yPos);
            }
        }

        protected void RedrawAllChildren(ref Graphics g) {
            foreach (TPComponent c in children) {
                c.Draw(ref g);
                TPPanel p = c as TPPanel;
                if (p != null)
                    p.RedrawAllChildren(ref g);
            }
        }

        internal override void Draw(ref Graphics g) {
            g.FillRectangle(BackgroundBrush, Bounds);
        }

        public override void Dispose() {
            base.Dispose();
            BackgroundBrush.Dispose();
        }
    }

}
