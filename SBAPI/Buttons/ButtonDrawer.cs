using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RZSB {
    namespace Buttons.Drawing {
        public interface ButtonDrawer : IDisposable{
            void DrawNormal(ref Bitmap bmp);
            void DrawPressed(ref Bitmap bmp);
        }
    }
}
