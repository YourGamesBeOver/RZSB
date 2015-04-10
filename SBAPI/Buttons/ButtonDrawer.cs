using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace RZSB {
    namespace Buttons.Drawing {
        public interface ButtonDrawer : IDisposable{
            void DrawNormal(ref Bitmap bmp);
            void DrawPressed(ref Bitmap bmp);
        }
    }
}
