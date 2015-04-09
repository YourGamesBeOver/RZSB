using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace RZSB {
    public class TouchpadProgressBar : IDisposable{

        public int NumberOfItems = 0;

        private int itemsCompleted = 0;
        public int ItemsCompleted {
            get { return itemsCompleted; }
            set {
                itemsCompleted = value;
                CompletionPercent = (float)ItemsCompleted / (float)NumberOfItems;
                if (DisableOnComplete && value >= NumberOfItems) {
                    this.Enabled = false;
                }
                Redraw();
            }
        }

        public float CompletionPercent {
            get;
            private set;
        }

        public bool DisableOnComplete = true;

        private string priv_desc = "";
        public string Description {
            get { return priv_desc; }
            set { 
                priv_desc = value;
                Redraw();
            }

        }
        public string Title;

        public Color BackgroundColor = Color.Black;
        public Color ForegroundColor = Color.Green;

        public int BarHeight = 50;
        public int BarWidth = 650;
        public float TitleFontSize = 20f;
        public float DescriptionFontSize = 15f;
        public string FontName = "Razer Regular";
        private Font titleFont;
        private Font descFont;

        private Rectangle BarOutlineRect;
        private Rectangle BarInnerRect;

        private SolidBrush foregroundBrush, backgroundBrush;

        public bool Enabled = true;

        private Bitmap bmp;

        private bool inBackground = false;

        public TouchpadProgressBar(int items, string title) {
            bmp = SBAPI.GenerateBitmapForTouchpad();


            NumberOfItems = items;
            Title = title;
            titleFont = new Font(FontName, TitleFontSize);
            descFont = new Font(FontName, DescriptionFontSize);
            BarOutlineRect = new Rectangle((800 - BarWidth) / 2, (480 - BarHeight) / 2, BarWidth, BarHeight);
            BarInnerRect = new Rectangle(BarOutlineRect.Location, BarOutlineRect.Size);
            BarInnerRect.Inflate(new Size(-2, -2));

            foregroundBrush = new SolidBrush(ForegroundColor);
            backgroundBrush = new SolidBrush(BackgroundColor);

            SBAPI.OnDeactivated += SBAPI_OnDeactivated;
            SBAPI.OnActivated += SBAPI_OnActivated;
            

            
        }

        void SBAPI_OnActivated() {
            inBackground = false;
            Redraw();
        }

        void SBAPI_OnDeactivated() {
            inBackground = true;
        }


        public void SetDescriptionAndIncrement(string newDesc) {
            priv_desc = newDesc;
            ItemsCompleted++;
        }

        public void Redraw() {
            if (inBackground) return;
            using (Graphics g = Graphics.FromImage(bmp)) {
                if (Enabled) {
                    //fill in the background
                    g.Clear(BackgroundColor);
                    //draw the outline of the bar
                    using (Pen p = new Pen(foregroundBrush)) {
                        g.DrawRectangle(p, BarOutlineRect);
                    }

                    //draw the inner box
                    BarInnerRect = new Rectangle(BarOutlineRect.Location, BarOutlineRect.Size);
                    BarInnerRect.Inflate(new Size(-2, -2));
                    float newWidth = BarInnerRect.Width * CompletionPercent;
                    BarInnerRect.Width = (int)Math.Round(newWidth);
                    g.FillRectangle(foregroundBrush, BarInnerRect);

                    //draw the labels
                    //first, the title label, located just above the bar
                    SizeF textSize = g.MeasureString(Title, titleFont);
                    float textY = BarOutlineRect.Y - textSize.Height;
                    float textX = (800 - textSize.Width) / 2;
                    g.DrawString(Title, titleFont, foregroundBrush, new PointF(textX, textY));

                    //now draw the description string!
                    SizeF descSize = g.MeasureString(Description, descFont);
                    float descY = BarOutlineRect.Y + BarOutlineRect.Height + 3;
                    float descX = (800 - descSize.Width) / 2;
                    g.DrawString(Description, descFont, foregroundBrush, new PointF(descX, descY));
                } else {
                    g.Clear(Color.Black);
                }
            }

            SBAPI.WriteBitmapImageToSB(SBDisplays.TRACKPAD, bmp);
        }

        public void Dispose() {
            foregroundBrush.Dispose();
            backgroundBrush.Dispose();
            descFont.Dispose();
            titleFont.Dispose();
            SBAPI.OnActivated -= SBAPI_OnActivated;
            SBAPI.OnDeactivated -= SBAPI_OnDeactivated;
        }
    }
}
