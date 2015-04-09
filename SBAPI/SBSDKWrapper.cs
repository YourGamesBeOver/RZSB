using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Razer.SwitchbladeSDK2
{
    #region SwitchBladeSDK_define.h

    /// <summary>
    /// The types of hardware supported by the Switchblade SDK 
    /// </summary>
    internal enum SWITCHBLADE_HARDWARE_TYPE : uint
    {
        HARDWARETYPE                = 0,
        HARDWARETYPE_SWITCHBLADE,	// switchblade module
        HARDWARETYPE_UNDEFINED
    };

    /// <summary>
    /// Values for the supported Dynamic key display 
    /// </summary>
    internal enum DYNAMICKEY_DISPAY_REGION
    {
        SWITCHBLADE_DYNAMIC_KEYS_PER_ROW    = 5,
        SWITCHBLADE_DYNAMIC_KEYS_ROWS	    = 2,
        SWITCHBLADE_DYNAMIC_KEY_X_SIZE		= 115,
        SWITCHBLADE_DYNAMIC_KEY_Y_SIZE		= 115,
        SWITCHBLADE_DK_SIZE_IMAGEDATA       = (SWITCHBLADE_DYNAMIC_KEY_X_SIZE * SWITCHBLADE_DYNAMIC_KEY_Y_SIZE * sizeof(ushort))
    };

    /// <summary>
    /// Values for the supported Touchpad display 
    /// </summary>
    internal enum TOUCHPAD_DISPLAY_REGION : uint
    {
        SWITCHBLADE_TOUCHPAD_X_SIZE			= 800,
        SWITCHBLADE_TOUCHPAD_Y_SIZE			= 480,
        SWITCHBLADE_TOUCHPAD_SIZE_IMAGEDATA = (SWITCHBLADE_TOUCHPAD_X_SIZE * SWITCHBLADE_TOUCHPAD_Y_SIZE * sizeof(ushort))
    };

    /// <summary>
    /// Values for the parameters in Switchblade SDK interfaces  
    /// </summary>
    internal enum SWITCHBLADESDK_DEFINE_CONSTANTS : int
    {
        SWITCHBLADE_DISPLAY_COLOR_DEPTH = 16, // 16 bpp
        MAX_STRING_LENGTH = 260 // no paths allowed longer than this
    };
    #endregion

    #region SwitchBladeSDK_types.h

    ////////////////////////////////////////////////////////////////////////////
    // SwitchBladeQueryCapabilities section
    // This SDK call tells about the hardware and resources we supply to
    // applications.
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Values for the SwitchBladeQueryCapabilities
    /// </summary>
    internal enum SWITCHBLADESDK_TYPE_CONSTANTS : int
    {
        MAX_SUPPORTED_SURFACES = 2,
        PIXEL_FORMAT_INVALID   = 0,
        PIXEL_FORMAT_RGB_565   = 1,
    };

    /// <summary>
    /// The structure containing the information regarding the Switchblade hardware 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RZSBSDK_QUERYCAPABILITIES
    {
	    public ulong                                            qc_version;
        public ulong                                            qc_BEVersion;
        public SWITCHBLADE_HARDWARE_TYPE                        qc_HardwareType;
        public ulong                                            qc_numSurfaces;
        [MarshalAs(UnmanagedType.ByValArray,SizeConst= 2)]
        public Point[]                                          qc_surfacegeometry;
        [MarshalAs(UnmanagedType.ByValArray,SizeConst= 2)]
        public ulong[]                                          qc_pixelformat;
        public Byte                                             qc_numDynamicKeys;
        public Point                                            qc_DynamicKeyArrangement;
        public Point                                            qc_keyDynamicKeySize;
    };

    ////////////////////////////////////////////////////////////////////////////
    // Common definitions section
    // definitions, enumerated types, helpful macros and callback info 
    // used in other sections 
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Possible states for a key (dynamic key) in the Switchblade device 
    /// This does not apply to a keyboard key. 
    /// </summary>
    internal enum RZSBSDK_KEYSTATETYPE
    {
        RZSBSDK_KEYSTATE_NONE = 0,
        RZSBSDK_KEYSTATE_UP,
        RZSBSDK_KEYSTATE_DOWN,
        RZSBSDK_KEYSTATE_HOLD,
        RZSBSDK_KEYSTATE_INVALID
    } ;

    /// <summary>
    /// Possible directions for gestures.  
    /// Currently only Flick supports direction. 
    /// </summary>
    internal enum RZSBSDK_DIRECTIONTYPE
    {
        RZSBSDK_DIRECTION_NONE = 0,
        RZSBSDK_DIRECTION_LEFT,
        RZSBSDK_DIRECTION_RIGHT,
        RZSBSDK_DIRECTION_UP,
        RZSBSDK_DIRECTION_DOWN,
        RZSBSDK_DIRECTION_INVALID
    } ;

    ////////////////////////////////////////////////////////////////////////////
    // Static keys section
    // definitions, enumerated types, helpful macros and callback info
    // Note that the events for static key is not currently supported. 
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This is the set of possible static keys.
    /// Note that zero is not a valid key value.
    /// </summary>
    internal enum RZSBSDK_STATICKEYTYPE : int
    {
        RZSBSDK_STATICKEY_NONE = 0,
        RZSBSDK_STATICKEY_RAZER,
        RZSBSDK_STATICKEY_GAME,
        RZSBSDK_STATICKEY_MACRO,
        RZSBSDK_STATICKEY_INVALID
    };

    ////////////////////////////////////////////////////////////////////////////
    // dynamic keys section
    // definitions, enumerated types, helpful macros and callback info
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This is the set of possible dynamic keys.
    /// Note that zero is not a valid key value.
    /// The RZSBSDK_DK_COUNT is the number of available keys 
    /// </summary>
    internal enum RZSBSDK_DKTYPE : int
    {
        RZSBSDK_DK_NONE = 0,
        RZSBSDK_DK_1,
        RZSBSDK_DK_2,
        RZSBSDK_DK_3,
        RZSBSDK_DK_4,
        RZSBSDK_DK_5,
        RZSBSDK_DK_6,
        RZSBSDK_DK_7,
        RZSBSDK_DK_8,
        RZSBSDK_DK_9,
        RZSBSDK_DK_10,
        RZSBSDK_DK_INVALID,
        RZSBSDK_DK_COUNT = 10
    };

    ////////////////////////////////////////////////////////////////////////////
    // Buffer Parameters section
    // Used for sending raw data of type PIXEL_BYTE to SwitchBlade subsystem
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Types of available display 
    /// RZSBSDK_DISPLAY_WIDGET is the Touchpad screen 
    /// RZSBSDK_DISPLAY_DK_1 to RZSBSDK_DISPLAY_DK_10 are dynamic keys
    /// </summary>
    internal enum RZSBSDK_DISPLAY 
    {
        RZSBSDK_DISPLAY_WIDGET  = ((1 << 16) | (0)),
        RZSBSDK_DISPLAY_DK_1    = ((1 << 16) | (1)),
        RZSBSDK_DISPLAY_DK_2    = ((1 << 16) | (2)),
        RZSBSDK_DISPLAY_DK_3    = ((1 << 16) | (3)),
        RZSBSDK_DISPLAY_DK_4    = ((1 << 16) | (4)),
        RZSBSDK_DISPLAY_DK_5    = ((1 << 16) | (5)),
        RZSBSDK_DISPLAY_DK_6    = ((1 << 16) | (6)),
        RZSBSDK_DISPLAY_DK_7    = ((1 << 16) | (7)),
        RZSBSDK_DISPLAY_DK_8    = ((1 << 16) | (8)),
        RZSBSDK_DISPLAY_DK_9    = ((1 << 16) | (9)),
        RZSBSDK_DISPLAY_DK_10   = ((1 << 16) | (10))
    };

    /// <summary>
    /// Supported Pixel format for the Switchblad displays 
    /// </summary>
    internal enum PIXEL_TYPE		
    { 
        RGB565 = 0    //16bppRGB565
    };

    /// <summary>
    /// The structure sent to the Switchblade device to update the 
    /// display using a bitmap data buffer 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RZSBSDK_BUFFERPARAMS
    {	
	    internal PIXEL_TYPE	PixelType;
	    internal int		    DataSize;	// Buffer size
        internal IntPtr       PtrData;
    };

    ///////////////////////////////////////////////////////////////////////////
    // Touchpad section
    // definitions, enumerated types, macros and callback info
    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This is the set of possible gestures.
    /// Note that zero is not a gestural value.
    /// 
    /// ALL is defined, but not supported, and we
    /// use in the indices the value UNDEFINED for
    /// anything we don't understand.
    /// </summary>
    internal enum RZSBSDK_GESTURETYPE : uint
    {
        RZSBSDK_GESTURE_NONE    = 0x00000000,
        RZSBSDK_GESTURE_PRESS   = 0x00000001, //dwParameters(touchpoints), wXPos(coordinate), wYPos(coordinate), wZPos(reserved)
        RZSBSDK_GESTURE_TAP     = 0x00000002, //dwParameters(reserved), wXPos(coordinate), wYPos(coordinate), wZPos(reserved)
        RZSBSDK_GESTURE_FLICK   = 0x00000004, //dwParameters(number of touch points), wXPos(reserved), wYPos(reserved), wZPos(direction)
        RZSBSDK_GESTURE_ZOOM    = 0x00000008, //dwParameters(1:zoomin, 2:zoomout), wXPos(), wYPos(), wZPos()
        RZSBSDK_GESTURE_ROTATE  = 0x00000010, //dwParameters(1:clockwise 2:counterclockwise), wXPos(), wYPos(), wZPos()
        RZSBSDK_GESTURE_MOVE    = 0x00000020, //dwParameters(reserved), wXPos(coordinate), wYPos(coordinate), wZPos(reserved)
        RZSBSDK_GESTURE_HOLD    = 0x00000040, //reserved
        RZSBSDK_GESTURE_RELEASE = 0x00000080, //dwParameters(touchpoints), wXPos(coordinate), wYPos(coordinate), wZPos(reserved)
        RZSBSDK_GESTURE_SCROLL  = 0x00000100, //reserved
        RZSBSDK_GESTURE_ALL     = 0xFFFF

    };

    ///////////////////////////////////////////////////////////////////////////
    // Application Events section
    // definitions, enumerated types, macros and callback info
    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Types of events that is supported by the SDK 
    /// </summary>
    internal enum RZSBSDK_EVENTTYPETYPE : int
    {
        RZSBSDK_EVENT_NONE = 0,
        RZSBSDK_EVENT_ACTIVATED,
        RZSBSDK_EVENT_DEACTIVATED,
        RZSBSDK_EVENT_CLOSE,
        RZSBSDK_EVENT_EXIT,
        RZSBSDK_EVENT_INVALID,	
    };

    #endregion

    #region SwitchBladeSDK_Error.h

    /// <summary>
    /// Standard HRESULT used by SDK 
    /// </summary>
    internal enum STANDARD_HRESULT : uint
    {
        S_OK                    = 0,
        E_FAIL                  = 0x80004005,
        E_INVALIDARG            = 0x80070057,
        E_POINTER               = 0x80004003,
        E_ABORT                 = 0x80004004,
        E_NOINTERFACE           = 0x80004002,
        E_NOTIMPL               = 0x8004001,
        ERROR_FILE_NOT_FOUND    = 0x2
    };

    /// <summary>
    /// Custom HRESULT used by the SDK 
    /// </summary>
    internal enum RZSBSDK_HRESULT : uint
    {
        RZSB_OK                     = STANDARD_HRESULT.S_OK,
        RZSB_UNSUCCESSFUL           = STANDARD_HRESULT.E_FAIL,
        RZSB_INVALID_PARAMETER      = STANDARD_HRESULT.E_INVALIDARG,
        RZSB_INVALID_POINTER        = STANDARD_HRESULT.E_POINTER,
        RZSB_ABORTED                = STANDARD_HRESULT.E_ABORT,
        RZSB_NO_INTERFACE           = STANDARD_HRESULT.E_NOINTERFACE,
        RZSB_NOT_IMPLEMENTED        = STANDARD_HRESULT.E_NOTIMPL,
        RZSB_FILE_NOT_FOUND         = STANDARD_HRESULT.ERROR_FILE_NOT_FOUND,

        RZSB_GENERIC_BASE           = 0x20000000,
        RZSB_FILE_ZERO_SIZE         = (RZSB_GENERIC_BASE + 0x1),  // zero-length files not allowed */
        RZSB_FILE_INVALID_NAME      = (RZSB_GENERIC_BASE + 0x2),  // */
        RZSB_FILE_INVALID_TYPE      = (RZSB_GENERIC_BASE + 0x3),  /* zero-sized images not allowed */
        RSZB_FILE_READ_ERROR        = (RZSB_GENERIC_BASE + 0x4),  /* tried to read X bytes, got back a different number. Chaos! */
        RZSB_FILE_INVALID_FORMAT    = (RZSB_GENERIC_BASE + 0x5),  /* not a supported file format */
        RZSB_FILE_INVALID_LENGTH    = (RZSB_GENERIC_BASE + 0x6),  /* file length not consistent with expected value */
        RZSB_FILE_NAMEPATH_TOO_LONG = (RZSB_GENERIC_BASE + 0x7),  /* path + name exceeds limit for the string, usually 260 chars */
        RZSB_IMAGE_INVALID_SIZE     = (RZSB_GENERIC_BASE + 0x8),  /* invalid image size -- totally wrong for dimensions */
        RZSB_IMAGE_INVALID_DATA     = (RZSB_GENERIC_BASE + 0x9),  /* image data did not verify as valid */
        RZSB_WIN_VERSION_INVALID	= (RZSB_GENERIC_BASE + 0xa),  /* must be Win7 or greater workstation */

        /* generic callback errors, but specific to the SDK */
        RZSB_CALLBACK_BASE          = 0x20010000,
        RZSB_CALLBACK_NOT_SET       = (RZSB_CALLBACK_BASE + 0x1),  /* tried to call or clear a callback that was not set */
        RZSB_CALLBACK_ALREADY_SET   = (RZSB_CALLBACK_BASE + 0x2),  /* tried to set a previously set callback without clearing it first */
        RZSB_CALLBACK_REMOTE_FAIL   = (RZSB_CALLBACK_BASE + 0x3),  /* set callback failed on the server side */
        /* control */
        RZSB_CONTROL_BASE_ERROR     = 0x20020000,
        RZSB_CONTROL_NOT_LOCKED     = (RZSB_CONTROL_BASE_ERROR + 0x01), /* unlock when we didn't lock? -- careless */
        RZSB_CONTROL_LOCKED         = (RZSB_CONTROL_BASE_ERROR + 0x02), /* someone else has the lock */
        RZSB_CONTROL_ALREADY_LOCKED = (RZSB_CONTROL_BASE_ERROR + 0x03), /* we already locked it? -- careless */  
        RZSB_CONTROL_PREEMPTED      = (RZSB_CONTROL_BASE_ERROR + 0x04), /* preemption took place! */

        /* dynamic keys */
        RZSB_DK_BASE_ERROR          = 0x20040000,
        RZSB_DK_INVALID_KEY         = (RZSB_DK_BASE_ERROR + 0x1), /* invalid dynamic key */
        RZSB_DK_INVALID_KEY_STATE   = (RZSB_DK_BASE_ERROR + 0x2), /* invalid dynamic key state */

        /* touchpad (buttons and gestures) */
        RZSB_TOUCHPAD_BASE_ERROR    = 0x20080000,
        RZSB_TOUCHPAD_INVALID_GESTURE = (RZSB_TOUCHPAD_BASE_ERROR + 0x1), /* invalid gesture */

        /* interface-specific errors */
        RZSB_INTERNAL_BASE_ERROR    = 0x20100000,
        RZSB_ALREADY_STARTED        = (RZSB_INTERNAL_BASE_ERROR + 0x1),  /* callback structures already initialized */
        RZSB_NOT_STARTED            = (RZSB_INTERNAL_BASE_ERROR + 0x2),  /* internal structure in disorder */
        RZSB_CONNECTION_ERROR       = (RZSB_INTERNAL_BASE_ERROR + 0x3),  /* connection to application services failed */
        RZSB_INTERNAL_ERROR         = (RZSB_INTERNAL_BASE_ERROR + 0x4),  /* unknown error -- catch-all for now */

        /* windows rendering errors */
        RZSB_WINRENDER_BASE_ERROR   = 0x20200000,
        RZSB_WINRENDER_OUT_OF_RESOURCES	= (RZSB_WINRENDER_BASE_ERROR + 0x01), /* could not allocate critical resources */
        RZSB_WINRENDER_THREAD_FAILED	= (RZSB_WINRENDER_BASE_ERROR + 0x02), /* could not start rendering thread */
        RZSB_WINRENDER_WRONG_MODEL		= (RZSB_WINRENDER_BASE_ERROR + 0x03)  /* not using multithreaded apartments */
    };
  
    #endregion

    #region SwitchBladeSDK

    /// <summary>
    /// The callback for Application events triggered by the SB framework (through the SDK). 
    /// </summary>
    /// <param name="rzEventType">Type of application event</param>
    /// <param name="dwParam1">Additional info depending on the event</param>
    /// <param name="dwParam2">Additional info depending on the event</param>
    /// <returns></returns>
    internal delegate RZSBSDK_HRESULT AppEventCallbackTypeDelegate(RZSBSDK_EVENTTYPETYPE rzEventType, UInt32 dwParam1, UInt32 dwParam2);
    
    /// <summary>
    /// The callback whenever a Dynamic key is pressed (or released) 
    /// </summary>
    /// <param name="dynamicKey">Affected key</param>
    /// <param name="dynamicKeyState">Indicates if key is pressed or released</param>
    /// <returns></returns>
    internal delegate RZSBSDK_HRESULT DynamicKeyCallbackDelegate(RZSBSDK_DKTYPE dynamicKey, RZSBSDK_KEYSTATETYPE dynamicKeyState);
    
    /// <summary>
    /// The callback for the gesture operations done on the trackpad. 
    /// This event will only trigger for gestures that were enabled with RzSBEnableGesture() call. 
    /// </summary>
    /// <param name="gesture">Type of gesture</param>
    /// <param name="dwParameters">Additional info depending on the gesture</param>
    /// <param name="wXPos">Additional info depending on the gesture</param>
    /// <param name="wYPos">Additional info depending on the gesture</param>
    /// <param name="wZPos">Additional info depending on the gesture</param>
    /// <returns></returns>
    internal delegate RZSBSDK_HRESULT TouchpadGestureCallbackDelegate(RZSBSDK_GESTURETYPE gesture, UInt32 dwParameters, UInt16 wXPos, UInt16 wYPos, UInt16 wZPos);
    
    /// <summary>
    /// The callback for the Keyboard events when keyboard capture is activated through the SDK 
    /// This event will get triggered only when RzSBCaptureKeyboard(true) is called. 
    /// </summary>
    /// <param name="uMsg">Indicates wether it is WM_KEYDOWN, WM_KEYUP or WM_CHAR</param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    internal delegate RZSBSDK_HRESULT KeyboardCallbackTypeDelegate(uint uMsg, UIntPtr wParam, IntPtr lParam);

    internal class NativeMethods
    {
        private const String DllName = "RzSwitchbladeSDK2.dll";

        /// <summary>
        /// Grants exclusive access to the SwitchBlade device, establishing application connections.
        /// RzSBStart must be called before other SwitchBlade SDK routines will succeed. 
        /// All calls for RzSBStart() must be matched with RzSBStop()
        /// </summary>
        /// <returns>HRESULT of the operation</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern RZSBSDK_HRESULT RzSBStart();

        /// <summary>
        /// Cleans up the SwitchBlade device connections and releases it for other applications
        /// All calls for RzSBStart() must be matched with RzSBStop().
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern void RzSBStop();

        /// <summary>
        /// Collects information about the SDK and the hardware supported. 
        /// </summary>
        /// <param name="capbilities">A pointer to a structure of type RZSBSDK_QUERYCAPABILITIES</param>
        /// <returns>HRESULT of the operation</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern RZSBSDK_HRESULT RzSBQueryCapabilities(IntPtr capabilities);

        /// <summary>
        /// Send bitmap data buffer directly to the Switchblade trackpad display
        /// </summary>
        /// <param name="targetDisplay">Specifies the target location on the SwitchBlade display</param>
        /// <param name="bufferParams">A pointer to a buffer parameter structure of type RZSBSDK_BUFFERPARAMS</param>
        /// <returns>HRESULT of the operation</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern RZSBSDK_HRESULT RzSBRenderBuffer(RZSBSDK_DISPLAY targetDisplay, IntPtr bufferParams);

        /// <summary>
        /// Set images on the SwitchBlade’s dynamic keys
        /// </summary>
        /// <param name="dynamicKey">The dynamic key rendering target</param>
        /// <param name="dynamicKeyState">The desired dynamic key state (RZSBSDK_KEYSTATE_UP, RZSBSDK_KEYSTATE_DOWN)</param>
        /// <param name="filename">The image filepath for the given state (see SWITCHBLADESDK_DEFINE_CONSTANTS for max character count)</param>
        /// <returns>HRESULT of the operation</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern RZSBSDK_HRESULT RzSBSetImageDynamicKey(RZSBSDK_DKTYPE dynamicKey, RZSBSDK_KEYSTATETYPE dynamicKeyState, 
                                                                    [MarshalAsAttribute(UnmanagedType.LPWStr)] string filename); 

        /// <summary>
        /// Places an image on the main SwitchBlade display
        /// </summary>
        /// <param name="filename">Filepath to the image to be placed on the main SwitchBlade display</param>
        /// <returns>HRESULT of the operation</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern RZSBSDK_HRESULT RzSBSetImageTouchpad([MarshalAsAttribute(UnmanagedType.LPWStr)] string filename);

        /// <summary>
        /// Enables or disables gesture events. 
        /// Once a gesture is enabled, the application will receive the gesture callbacks. 
        /// Default is to enable all gesture events. 
        /// </summary>
        /// <param name="gestureType">Gesture to be enabled or disabled</param>
        /// <param name="bEnable">The enable state</param>
        /// <returns>HRESULT of the operation</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern RZSBSDK_HRESULT RzSBEnableGesture(RZSBSDK_GESTURETYPE gestureType, bool bEnable);

        /// <summary>
        /// Enables or disables gesture event forwarding to the OS.
        /// Once a gesture is enabled, the OS will recieve it. 
        /// </summary>
        /// <param name="gestureType">Gesture to be enabled or disabled</param>
        /// <param name="bEnable">The enable state</param>
        /// <returns>HRESULT of the operation</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern RZSBSDK_HRESULT RzSBEnableOSGesture(RZSBSDK_GESTURETYPE gestureType, bool bEnable);

        /// <summary>
        /// Enables or disables the keyboard capture functionality.
        /// Once enabled, only the application can receive keyboard input from the SB device. 
        /// </summary>
        /// <param name="bEnable">The enable state.</param>
        /// <returns>HRESULT of the operation</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern RZSBSDK_HRESULT RzSBCaptureKeyboard(bool bEnable);

        /// <summary>
        /// Sets the callback function for application event callbacks
        /// </summary>
        /// <param name="callback">delegate to recieve the callback</param>
        /// <returns>HRESULT of the operation</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern RZSBSDK_HRESULT RzSBAppEventSetCallback(AppEventCallbackTypeDelegate callback);

        /// <summary>
        /// Sets the callback function for dynamic key events.
        /// </summary>
        /// <param name="callback">delegate to recieve the callback</param>
        /// <returns>HRESULT of the operation</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern RZSBSDK_HRESULT RzSBDynamicKeySetCallback(DynamicKeyCallbackDelegate callback);

        /// <summary>
        /// Sets the callback function for gesture events.
        /// </summary>
        /// <param name="callback">delegate to recieve the callback</param>
        /// <returns>HRESULT of the operation</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern RZSBSDK_HRESULT RzSBGestureSetCallback(TouchpadGestureCallbackDelegate callback);

        /// <summary>
        /// Sets the callback function for keyboard events.
        /// </summary>
        /// <param name="callback">delegate to recieve the callback</param>
        /// <returns>HRESULT of the operation</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern RZSBSDK_HRESULT RzSBKeyboardCaptureSetCallback(KeyboardCallbackTypeDelegate callback);

    }

    #endregion
}
