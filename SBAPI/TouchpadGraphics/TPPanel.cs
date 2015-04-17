using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RZSB.TouchpadGraphics {
    public class TPPanel : TPComponent{
        protected List<TPComponent> children = new List<TPComponent>();

        public Color BackgroundColor {
            get { return BackgroundBrush.Color; }
            set {
                BackgroundBrush.Dispose();
                BackgroundBrush = new SolidBrush(value);
                RequestTotalRedraw();
            }
        }
        private SolidBrush BackgroundBrush = new SolidBrush(DEFAULT_BACKGROUND_COLOR);

        private Pen BorderPen = new Pen(DEFAULT_FOREGROUND_COLOR, 3f);
        public Color BorderColor {
            get { return BorderPen.Color; }
            set {
                Pen oldPen = BorderPen;
                BorderPen = new Pen(value, BorderWidth);
                oldPen.Dispose();
                RequestTotalRedraw();
            }
        }
        public float BorderWidth {
            get { return BorderPen.Width; }
            set {
                Pen oldPen = BorderPen;
                BorderPen = new Pen(BorderColor, value);
                oldPen.Dispose();
                RequestTotalRedraw();
            }
        }
        private bool priv_drawBorder = false;
        public bool DrawBorder {
            get { return priv_drawBorder; }
            set {
                if (priv_drawBorder != value) {
                    priv_drawBorder = value;
                    RequestTotalRedraw();
                }
            }
        }

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
                if (c.ContainsPoint(p) && c.Enabled) return c;
            }
            return null;
        }

        internal override void Tapped(int xPos, int yPos) {
            TPComponent child = GetChildAtPosition(new Point(xPos, yPos));
            if (child != null) {
                child.Tapped(xPos - child.Position.X, yPos - child.Position.Y);
            } else {
                base.Tapped(xPos, yPos);
            }
        }

        internal override void Pressed(uint touches, int xPos, int yPos) {
            TPComponent child = GetChildAtPosition(new Point(xPos, yPos));
            if (child != null) {
                child.Pressed(touches, xPos - child.Position.X, yPos - child.Position.Y);
            } else {
                base.Pressed(touches, xPos, yPos);
            }
        }

        internal override void Released(uint touches, int xPos, int yPos) {
            TPComponent child = GetChildAtPosition(new Point(xPos, yPos));
            if (child != null) {
                child.Released(touches, xPos - child.Position.X, yPos - child.Position.Y);
            } else {
                base.Released(touches, xPos, yPos);
            }
        }

        internal override void FingerOver(int xPos, int yPos) {
            TPComponent child = GetChildAtPosition(new Point(xPos, yPos));
            if (child != null) {
                child.FingerOver(xPos - child.Position.X, yPos - child.Position.Y);
            } else {
                base.FingerOver(xPos, yPos);
            }
        }

        internal virtual void RedrawAllChildren(ref Graphics g) {
            foreach (TPComponent c in children) {
                if (c.Enabled) {
                    var container = g.BeginContainer(new Rectangle(c.Position.X, c.Position.Y, Size.Width, Size.Height),ToRect(Size),  GraphicsUnit.Pixel);
                    g.SetClip(ToRect(Size));
                    c.Draw(ref g);
                    TPPanel p = c as TPPanel;
                    if (p != null)
                        p.RedrawAllChildren(ref g);
                    g.EndContainer(container);
                    
                }
            }
        }

        internal override void Draw(ref Graphics g) {
            g.FillRectangle(BackgroundBrush, ToRect(Size));
            if (DrawBorder) {
                g.DrawRectangle(BorderPen, ToRect(Size));
            }
        }

        protected override void DisposeManagedResources() {
            base.DisposeManagedResources();
            BackgroundBrush.Dispose();
            BorderPen.Dispose();
        }
    }

}
