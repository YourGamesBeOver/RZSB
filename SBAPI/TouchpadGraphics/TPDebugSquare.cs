﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RZSB.TouchpadGraphics {
    public class TPDebugSquare : TPComponent {

        private SolidBrush PrimaryBrush = new SolidBrush(DEFAULT_BACKGROUND_COLOR);
        public Color PrimaryColor {
            get { return PrimaryBrush.Color; }
            set {
                PrimaryBrush.Dispose();
                PrimaryBrush = new SolidBrush(value);
            }
        }

        private SolidBrush SecondaryBrush = new SolidBrush(DEFAULT_FOREGROUND_COLOR);
        public Color SecondaryColor {
            get { return SecondaryBrush.Color; }
            set {
                SecondaryBrush.Dispose();
                SecondaryBrush = new SolidBrush(value);
            }
        }
        

        public TPDebugSquare(Rectangle bounds) {
            Bounds = bounds;
            OnFingerOver += TPDebugSquare_OnFingerOver;
        }

        void TPDebugSquare_OnFingerOver(ushort xPos, ushort yPos) {
            //Util.Utils.println("FLIP!");
            SolidBrush temp = PrimaryBrush;
            PrimaryBrush = SecondaryBrush;
            SecondaryBrush = temp;
        }

        internal override void Draw(ref Graphics g) {
            g.FillRectangle(PrimaryBrush, Bounds);
        }

        public override void Dispose() {
            base.Dispose();
            PrimaryBrush.Dispose();
            SecondaryBrush.Dispose();
        }
    }
}
