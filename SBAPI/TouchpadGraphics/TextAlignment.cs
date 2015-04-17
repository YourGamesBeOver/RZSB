using System;

namespace RZSB.TouchpadGraphics {
    [Flags]
    public enum TextAlignment {
        NONE =          0,//undefined...
        LEFT =          1,
        RIGHT =         1 << 1,
        HORIZ_CENTER =  LEFT | RIGHT,
        TOP =           1 << 2,
        BOTTOM =        1 << 3,
        VERT_CENTER =   TOP | BOTTOM,
        CENTER =        HORIZ_CENTER | VERT_CENTER,
        DEFAULT =       LEFT | TOP
    }
}
