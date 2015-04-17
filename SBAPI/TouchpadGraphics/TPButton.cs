using System;
using System.Drawing;

namespace RZSB.TouchpadGraphics {
    public class TPButton : TPSimpleLabel {
        public bool Pressed {
            get;
            protected set;
        }

        public TPButton(Point position, string text) : base(position, text) {

        }


    }
}
