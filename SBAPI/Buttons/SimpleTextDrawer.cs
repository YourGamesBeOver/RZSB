using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace RZSB.Buttons {
    namespace Drawing {
        public class SimpleTextButtonDrawer : ButtonDrawer{

            public static Color DEFAULT_TEXT_COLOR = Color.FromArgb(0, 200, 0);
            public static Color DEFAULT_BACKGROUND_COLOR = Color.Black;
            public const string DEFAULT_FONT_NAME = "Consolas";

            public Font font{ get; private set; }
            public Color TextColor { get; private set; }
            public Color BackgroundColor { get; private set; }
            public string Text { get; private set; }

            private Bitmap normalBmp = SBAPI.GenerateBitmapForDK();
            private Bitmap pressedBmp; //initialized in repaint()

            private bool inConstructor = true;//flag to keep the image from being updated excessively during initial construction

            public SimpleTextButtonDrawer(string text, Color textColor, Color backgroundColor, string fontName = DEFAULT_FONT_NAME){
                if (textColor == null) textColor = DEFAULT_TEXT_COLOR;
                if (backgroundColor == null) backgroundColor = DEFAULT_BACKGROUND_COLOR;
                if (fontName == null) fontName = DEFAULT_FONT_NAME;
                setTextColor(textColor);
                setBackgroundColor(backgroundColor);
                setFont(fontName);
                setText(text);
                inConstructor = false;
                repaint();
            }

            public void Dispose() {
                normalBmp.Dispose();
                if (pressedBmp != null) pressedBmp.Dispose();
                font.Dispose();
            }

            public void setText(string newText) {
                this.Text = newText;
                repaint();
            }
            public void setTextColor(Color c) {
                TextColor = c;
                repaint();
            }

            public void setFont(string fontName) {
                if (this.font != null) this.font.Dispose();
                this.font = new Font(fontName, 1f);
                repaint();
            }

            public void setBackgroundColor(Color c) {
                BackgroundColor = c;
                repaint();
            }

            private void repaint() {
                if (inConstructor) return;
                DrawFirstPass(ref normalBmp);
                pressedBmp = normalBmp.Clone() as Bitmap;
                DrawSecondPass(ref pressedBmp);
            }

            private void DrawFirstPass(ref Bitmap bmp) {
                using (Graphics g = Graphics.FromImage(bmp)) {
                    using (Brush bgb = new SolidBrush(BackgroundColor)) {
                        g.FillRegion(bgb, new Region(new Rectangle(0, 0, bmp.Width, bmp.Height)));
                    }
                    using (Brush textBrush = new SolidBrush(TextColor)) {
                        Font f = Util.Utils.FindFont(g, Text, bmp.Size, font);
                        SizeF size = g.MeasureString(Text, f);
                        float x = (bmp.Width - size.Width) / 2f;
                        float y = (bmp.Height - size.Height) / 2f;
                        g.DrawString(Text, f, textBrush, new PointF(x, y));
                    }
                }
            }
            private void DrawSecondPass(ref Bitmap bmp) {
                //DrawNormal(ref bmp);
                Blur(ref bmp, 3);
            }

            public void DrawNormal(ref Bitmap bmp) {
                bmp = normalBmp;
            }

            public void DrawPressed(ref Bitmap bmp) {
                bmp = pressedBmp;
            }

            private static void Blur(ref Bitmap image, Int32 blurSize) {
                Bitmap blurred = new Bitmap(image.Width, image.Height);
                //Rectangle rectangle = new Rectangle(0, 0, image.Width, image.Height);


                // make an exact copy of the bitmap provided
                using (Graphics graphics = Graphics.FromImage(blurred))
                    graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                        new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

                // look at every pixel in the blur rectangle
                for (Int32 xx = 0; xx < image.Width; xx++) {
                    for (Int32 yy = 0; yy < image.Height; yy++) {
                        Int32 avgR = 0, avgG = 0, avgB = 0;
                        Int32 blurPixelCount = 0;

                        // average the color of the red, green and blue for each pixel in the
                        // blur size while making sure you don't go outside the image bounds
                        for (Int32 x = xx; (x < xx + blurSize && x < image.Width); x++) {
                            for (Int32 y = yy; (y < yy + blurSize && y < image.Height); y++) {
                                Color pixel = image.GetPixel(x, y);

                                avgR += pixel.R;
                                avgG += pixel.G;
                                avgB += pixel.B;

                                blurPixelCount++;
                            }
                        }

                        avgR = avgR / blurPixelCount;
                        avgG = avgG / blurPixelCount;
                        avgB = avgB / blurPixelCount;

                        // now that we know the average for the blur size, set each pixel to that color
                        for (Int32 x = xx; x < xx + blurSize && x < image.Width; x++)
                            for (Int32 y = yy; y < yy + blurSize && y < image.Height; y++)
                                image.SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
                    }
                }
            }
        }

    }
}
