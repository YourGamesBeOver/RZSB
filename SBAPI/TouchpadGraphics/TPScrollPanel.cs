using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RZSB.TouchpadGraphics {
    public class TPScrollPanel : TPPanel{
        private Rectangle priv_innerBounds;
        public Rectangle ContentBounds {
            get { return priv_innerBounds; }
            private set { priv_innerBounds = value; }
        }

        public float VerticalScroll = 0f, HorizontalScroll = 0f;

        private int lastX, lastY;
        private bool dragging = false;


        public TPScrollPanel(Rectangle bounds) {
            this.Position = bounds.Location;
            this.Size = bounds.Size;
            ContentBounds = new Rectangle(new Point(0, 0), bounds.Size);

            SBAPI.OnReleaseGesture += SBAPI_OnReleaseGesture;
        }

        void SBAPI_OnReleaseGesture(uint touchpoints, ushort xPos, ushort yPos) {
            dragging = false;
        }

        internal override void RedrawAllChildren(ref Graphics g) {
            var contcontainer = g.BeginContainer(new Rectangle(Position, Size), new Rectangle(Position.X - (int)HorizontalScroll, Position.Y - (int)VerticalScroll, Size.Width, Size.Height), GraphicsUnit.Pixel);
            g.SetClip(new Rectangle(-(int)HorizontalScroll, -(int)VerticalScroll, Size.Width, Size.Height));
            base.RedrawAllChildren(ref g);
            g.EndContainer(contcontainer);
        }

        internal override void Pressed(uint touches, int xPos, int yPos) {
            dragging = true;
            lastX = xPos;
            lastY = yPos;
            base.Pressed(touches, xPos, yPos);
        }

        private void calculateInnerBounds() {
            int maxX = 0;
            int maxY = 0;
            foreach (TPComponent c in children) {
                int cMaxX = c.Position.X + c.Size.Width;
                if (cMaxX > maxX) maxX = cMaxX;
                int cMaxY = c.Position.Y + c.Size.Height;
                if (cMaxY > maxY) maxY = cMaxY;
            }
            ContentBounds = new Rectangle(0, 0, maxX, maxY);
        }

        public override void Add(TPComponent newChild) {
            base.Add(newChild);
            calculateInnerBounds();
            RequestTotalRedraw();
        }

        public override void Remove(TPComponent oldChild) {
            base.Remove(oldChild);
            calculateInnerBounds();
            RequestTotalRedraw();
        }

        internal override void FingerOver(int xPos, int yPos) {
            if (dragging) {
                calculateInnerBounds();
                if (ContentBounds.Width > Size.Width) {
                    float deltaX = xPos - lastX;
                    HorizontalScroll += deltaX;
                    if (HorizontalScroll < Size.Width - ContentBounds.Width) HorizontalScroll = Size.Width - ContentBounds.Height;
                    if (HorizontalScroll > 0f) HorizontalScroll = 0f;
                } else {
                    HorizontalScroll = 0f;
                }

                if (ContentBounds.Height > Size.Height) {
                    float deltaY = yPos - lastY;
                    VerticalScroll += deltaY;
                    if (VerticalScroll < Size.Height - ContentBounds.Height) VerticalScroll = Size.Height - ContentBounds.Height;
                    if (VerticalScroll > 0f) VerticalScroll = 0f;
                } else {
                    VerticalScroll = 0f;
                }
                lastX = xPos;
                lastY = yPos;
            } else {
                base.FingerOver(xPos, yPos);
            }
        }
    }
}
