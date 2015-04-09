using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Razer.SwitchbladeSDK2;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using RZSB.Util;

namespace RZSB {

    #region enums
    //some enums that are much cleaner than the RZSBSDK ones
    public enum SBDisplays : int{
        TRACKPAD = 0,
        DK_1,
        DK_2,
        DK_3,
        DK_4,
        DK_5,
        DK_6,
        DK_7,
        DK_8,
        DK_9,
        DK_10,
        NUMBER_OF_DISPLAYS
    }

    public enum SBEvent : int {
        NONE = 0,
        ACTIVATED,
        DEACTIVATED,
        CLOSE,
        EXIT,
        INVALID
    }
    public enum FlickDirection : int {
        NONE = 0,
        LEFT,
        RIGHT,
        UP,
        DONW,
        INVALID
    }

    #endregion


    /************************************
     * SBAPI
     * A wrapper for the Razer Switchblade SDK C# wrapper
     * Yes, I know how stupid that sounds, a wrapper for a wrapper? Why would you ever need that?
     * Well, because Razer sucks at software, that's why.  
     * The default C# 'wrapper' is barely a wrapper at all, it just exposes the C++ functions and exactly copies the structs into C#, 
     * but doesn't actually adapt them to the C# style.
     * 
     * This class provides a much easier, and more useful interface to Razer's RZSBSDKAPI class.  
     * 
     * This class is also thread safe; its functions can be called from any thread!
     * This class also ensures that the SB is stopped in the event of a crash (unhandled exception) or graceful termination
     * 
     * Some jargon and shorthand:
     *  SB - Switchblade
     *  RZ - Razer
     *  RZSBSDKAPI - the class, written by Razer, defined in RZSBSDKWrapper.cs.  
     *      It contains all the static c++ functions used to control the SB.  Its C# class name is NativeMethods
     *  DK - Dynamic Key; the 10 keys at the top of the SB
     *  Widget - the trackpad screen (for some reason, Razer occasionally refers to the trackpad screen as the 'Widget')
     *  TP - Trackpad (or the trackpad screen, I use 'TP' to refer to either; You can usually figure out which one from the context)
     *  
     * 
     * 
     ************************************/
    public class SBAPI {
        //the number of screens, total, on the SB
        private const int NUMBER_OF_DISPLAYS = (int)SBDisplays.NUMBER_OF_DISPLAYS;
        
        //these are part of the windows API
        private const UInt32 WM_KEYDOWN = 0x100;
        private const UInt32 WM_KEYUP = 0x101;
        private const UInt32 WM_CHAR = 0x102;

        #region rz constants
        //some constants that Razer thought would be a good idea to keep in enums because they're stupid
        public const int DK_WIDTH = (int)DYNAMICKEY_DISPAY_REGION.SWITCHBLADE_DYNAMIC_KEY_X_SIZE;
        public const int DK_HEIGHT = (int)DYNAMICKEY_DISPAY_REGION.SWITCHBLADE_DYNAMIC_KEY_Y_SIZE;
        public const int DK_IMAGEDATA_SIZE = (int)DYNAMICKEY_DISPAY_REGION.SWITCHBLADE_DK_SIZE_IMAGEDATA;
        public const int DKS_PER_ROW = (int)DYNAMICKEY_DISPAY_REGION.SWITCHBLADE_DYNAMIC_KEYS_PER_ROW;
        public const int DK_ROW_COUNT = (int)DYNAMICKEY_DISPAY_REGION.SWITCHBLADE_DYNAMIC_KEYS_ROWS;
        public const int DK_COUNT = DK_ROW_COUNT * DKS_PER_ROW;

        public const int TP_WIDTH = (int)TOUCHPAD_DISPLAY_REGION.SWITCHBLADE_TOUCHPAD_X_SIZE;
        public const int TP_HEIGHT = (int)TOUCHPAD_DISPLAY_REGION.SWITCHBLADE_TOUCHPAD_Y_SIZE;
        public const int TP_IMAGEDATA_SIZE = (int)TOUCHPAD_DISPLAY_REGION.SWITCHBLADE_TOUCHPAD_SIZE_IMAGEDATA;

        public const PixelFormat SB_PIXELFORMAT = PixelFormat.Format16bppRgb565;

        #endregion

        public static bool Started {
            get;
            private set;
        }

        //mutex to ensure that two threads don't access the SB at the same time
        private static Mutex SBMux = new Mutex();



        #region Event and Delegate Declarations
        //*** EVENTS AND DELEGATES ***//

        //private event only used internally
        private static event AppEventCallbackTypeDelegate appEventCallbackDelegate;
        private static event DynamicKeyCallbackDelegate dynamicKeyCallbackDelegate;
        private static event TouchpadGestureCallbackDelegate gestureCallbackDelegate;
        private static event KeyboardCallbackTypeDelegate keyboardCallbackDelegate;

        //public events so other classes can more directly recieve SB callbacks
        public delegate void SimpleDynamicKeyCallbackDelegate(int key, bool down);
        public static event SimpleDynamicKeyCallbackDelegate OnDynamicKeyEvent;
        public delegate void SimpleAppEventCallbackDelegate(SBEvent evnt);//there are also convience delegates below
        public static event SimpleAppEventCallbackDelegate OnAppEvent;
        //public static event TouchpadGestureCallbackDelegate publicTouchpadGestureCallback;
        //To keep things more simple, I have split Touchpad Gestures up into individual callbacks below

        //public events and delegates for each type of TouchpadGesture, seperate
        //events for convience, and so that there are no extraneous parameters
        public delegate void IntXYGestureDelegate(uint touchpoints, ushort xPos, ushort yPos);
        public delegate void XYGestureDelegate(ushort xPos, ushort yPos);
        public delegate void FlickGestureDelegate(uint touches, FlickDirection direction);
        public delegate void ZoomGestureDelegate(bool zoomIn);
        public delegate void RotateGestureDelegate(bool clockwise);
        //called when a finger initially touches the touchpad
        public static event IntXYGestureDelegate OnPressGesture;
        //called when a user removes a finger from the touchpad
        public static event IntXYGestureDelegate OnReleaseGesture;
        //called when a user taps the touchpad
        public static event XYGestureDelegate OnTapGesture;
        //called for flicks; only the direction and the number of fingers is known
        public static event FlickGestureDelegate OnFlickGesture;
        //called when the user pinches or spreads two fingers on the touchpad; false is passed in for a pinch, true for a spread
        public static event ZoomGestureDelegate OnZoomGesture;
        //called when the user places one finger on the touchpad and then rotates another around it, true will be passed in if it was clockwise
        public static event RotateGestureDelegate OnRotateGesture;
        //called when the user moves a finger on the touchpad
        public static event XYGestureDelegate OnMoveGesture;

        //public events and delegates for SB events
        public delegate void SBEventDelegate();
        public static event SBEventDelegate OnActivated;
        public static event SBEventDelegate OnDeactivated;
        public static event SBEventDelegate OnClose;
        public static event SBEventDelegate OnExit;

        public static event SBEventDelegate OnCaptureKeyboard;
        public static event SBEventDelegate OnReleaseKeyboard;

        //public keyboard delegates and events
        public delegate void KeyboardKeyTypedDelegate(char key, IntPtr modifier);
        public delegate void KeyboardKeyDelegate(VK key, IntPtr modifier);
        public static event KeyboardKeyDelegate OnKeyDown;
        public static event KeyboardKeyDelegate OnKeyUp;
        public static event KeyboardKeyTypedDelegate OnKeyTyped;


        #endregion

        #region Image Handling

        private static Bitmap DEFAULT_DK_IMAGE, DEFAULT_TRACKPAD_IMAGE;
        private static Color BACKGROUND_COLOR = Color.Black;


        //*** Buffers for each button and for the touchpad ***//
        //index 0 is the touchpad, the others correspond to the DKs in RZSBSDK_DKTYPE
        private static IntPtr[] bufferParamsPtrs = new IntPtr[NUMBER_OF_DISPLAYS];
        private static IntPtr[] imageDataPtrs = new IntPtr[NUMBER_OF_DISPLAYS];

        //*** Images and other stuff for the buttons and trackpad ***//
        private static bool[] useImages = new bool[NUMBER_OF_DISPLAYS];
        private static string[,] imageFilenames = new string[NUMBER_OF_DISPLAYS, 2]; //position 0,1 is unused, the touchpad doesn't have a 'pressed' image

        //mutexes to protect the image buffers, and the filenames and imageUse bools
        private static Mutex[] bufferMuxes = new Mutex[NUMBER_OF_DISPLAYS];

        private static void WriteBitmapDataToImageDataPtr(Bitmap bmp, int index) {
            //Get a copy of the byte array you need to send to your tft
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
            //Marshal.Copy(bmpData.Scan0, imgArray, 0, imgArray.Length);
            Utils.NativeMethods.memcpy(imageDataPtrs[index], bmpData.Scan0, (UIntPtr)(bmp.Width * bmp.Height * 2));
            bmp.UnlockBits(bmpData);
        }

        public static void SendBufferToSB(SBDisplays display) {
            chkStrt();
            //a thread can wait on a mutex it already owns, so long as it releases it the same number of times as it waits on it
            //the extra layer of mutex waits is in case this function is called from somewhere other than WriteBitmapImagesToSB
            bufferMuxes[(int)display].WaitOne();
            SBMux.WaitOne();
            useImages[(int)display] = false;
            RZSBSDKCheck(NativeMethods.RzSBRenderBuffer(convertToRZSBSDKDisplay(display), bufferParamsPtrs[(int)display]));
            SBMux.ReleaseMutex();
            bufferMuxes[(int)display].ReleaseMutex();
        }

        public static void WriteBitmapImageToSB(SBDisplays display, Bitmap bmp) {
            chkStrt();
            bufferMuxes[(int)display].WaitOne();
            WriteBitmapDataToImageDataPtr(bmp, (int)display);
            SendBufferToSB(display);
            bufferMuxes[(int)display].ReleaseMutex();
        }

        public static void SendImageToTouchpad(string filename) {
            chkStrt();
            bufferMuxes[0].WaitOne();
            SBMux.WaitOne();
            useImages[0] = true;
            imageFilenames[0, 0] = filename;
            RZSBSDKCheck(NativeMethods.RzSBSetImageTouchpad(filename));
            SBMux.ReleaseMutex();
            bufferMuxes[0].ReleaseMutex();
        }

        //key must be between 1 and 10 (the actual DK number)
        public static void SendImageToDK(int key, bool pressed, string filename) {
            chkStrt();
            bufferMuxes[key].WaitOne();
            SBMux.WaitOne();
            useImages[key] = true;
            imageFilenames[key, pressed ? 1 : 0] = filename;
            RZSBSDKCheck(NativeMethods.RzSBSetImageDynamicKey(
                (RZSBSDK_DKTYPE)key,
                pressed ? RZSBSDK_KEYSTATETYPE.RZSBSDK_KEYSTATE_DOWN : RZSBSDK_KEYSTATETYPE.RZSBSDK_KEYSTATE_UP,
                filename));
            SBMux.ReleaseMutex();
            bufferMuxes[key].ReleaseMutex();
        }

        public static void RefreshAll() {
            chkStrt();
            foreach (Mutex m in bufferMuxes) m.WaitOne();
            SBMux.WaitOne();
            //trackpad
            if (useImages[0]) {
                RZSBSDKCheck(NativeMethods.RzSBSetImageTouchpad(imageFilenames[0, 0]));
            } else {
                RZSBSDKCheck(NativeMethods.RzSBRenderBuffer(convertToRZSBSDKDisplay(SBDisplays.TRACKPAD), bufferParamsPtrs[0]));
            }
            //DKs
            for (int i = 1; i < NUMBER_OF_DISPLAYS; i++) {
                if (useImages[i]) {
                    SendImageToDK(i, false, imageFilenames[i, 0]);
                } else {
                    RZSBSDKCheck(NativeMethods.RzSBRenderBuffer(convertToRZSBSDKDisplay((SBDisplays)i), bufferParamsPtrs[i]));
                }
            }

            SBMux.ReleaseMutex();
            foreach (Mutex m in bufferMuxes) m.ReleaseMutex();
        }

        public static void ClearDisplay(SBDisplays display) {
            WriteBitmapImageToSB(display, display == SBDisplays.TRACKPAD ? DEFAULT_TRACKPAD_IMAGE : DEFAULT_DK_IMAGE);
        }

        private static RZSBSDK_DISPLAY convertToRZSBSDKDisplay(SBDisplays dsp) {
            return (RZSBSDK_DISPLAY)((1 << 16) | ((int)dsp));
        }
        #endregion

        #region startup and shutdown functions

        /// <summary>
        /// starts the SB
        /// </summary>
        public static void Start() {
            SBMux.WaitOne();
            if (Started) {
                SBMux.ReleaseMutex();
                return;
            }
            setupDisplayBuffers();
            RegisterShutdownCallbacks();
            RZSBSDKCheck(NativeMethods.RzSBStart());
            RegisterSBCallbacks();
            if (DEFAULT_DK_IMAGE == null) {
                DEFAULT_DK_IMAGE = GenerateBitmapForDK();
                using (Graphics g = Graphics.FromImage(DEFAULT_DK_IMAGE)){
                    g.Clear(BACKGROUND_COLOR);
                }
            }
            if (DEFAULT_TRACKPAD_IMAGE == null) {
                DEFAULT_TRACKPAD_IMAGE = GenerateBitmapForTouchpad();
                using (Graphics g = Graphics.FromImage(DEFAULT_TRACKPAD_IMAGE)) {
                    g.Clear(BACKGROUND_COLOR);
                }
            }
            Started = true;
            SBMux.ReleaseMutex();
        }

        /// <summary>
        /// stops the SB
        /// </summary>
        public static void Stop() {
            SBMux.WaitOne();
            if (!Started) {
                SBMux.ReleaseMutex();
                return;
            }
            forceStop();

            SBMux.ReleaseMutex();
        }

        private static void forceStop() {
            //first things first: stop listening to the SB
            DeregisterSBCallbacks();
            //this RZSBSDKAPI call doesn't need RZSBSDKCheck because it doesn't return anything
            NativeMethods.RzSBStop(); //stop the SB
            DEFAULT_DK_IMAGE.Dispose();
            DEFAULT_TRACKPAD_IMAGE.Dispose();
            DEFAULT_DK_IMAGE = null;
            DEFAULT_TRACKPAD_IMAGE = null;
            cleanupDisplayBuffers();
            Started = false;
            DeregisterShutdownCallbacks();
        }

        #region functions for registering and deregistering callbacks

        private static void RegisterSBCallbacks() {
            Utils.print("Registering RzSB callbacks...");
            appEventCallbackDelegate += appEventCallback;
            dynamicKeyCallbackDelegate += dynamicKeyCallback;
            gestureCallbackDelegate += gestureCallback;
            keyboardCallbackDelegate += keyboardCallback;
            
            RZSBSDKCheck(NativeMethods.RzSBAppEventSetCallback(appEventCallbackDelegate));
            RZSBSDKCheck(NativeMethods.RzSBDynamicKeySetCallback(dynamicKeyCallbackDelegate));
            RZSBSDKCheck(NativeMethods.RzSBGestureSetCallback(gestureCallbackDelegate));
            RZSBSDKCheck(NativeMethods.RzSBKeyboardCaptureSetCallback(keyboardCallbackDelegate));

            OnActivated += RefreshAll;

            Utils.println("Done!\n", ConsoleColor.DarkGreen);
        }

        private static void DeregisterSBCallbacks() {
            appEventCallbackDelegate -= appEventCallback;
            dynamicKeyCallbackDelegate -= dynamicKeyCallback;
            gestureCallbackDelegate -= gestureCallback;
            keyboardCallbackDelegate -= keyboardCallback;

            OnActivated -= RefreshAll;
        }


        private static void RegisterShutdownCallbacks() {
            Utils.print("Registering shutdown callbacks...");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Utils.println("Done!\n", ConsoleColor.DarkGreen);
        }

        private static void DeregisterShutdownCallbacks() {
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
        }

        #region ShutdownCallbacks
        static void CurrentDomain_ProcessExit(object sender, EventArgs e) {
            Utils.println("Shutting down RzSB due to process exit");
            Stop();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Utils.println("Shutting down RzSB due to unhandled exception!", ConsoleColor.Red);
            forceStop();
        }
        #endregion

        #endregion

        #region Display Buffer Setup
        private static void setupDisplayBuffers() {
            //setup touchpad display buffers
            imageDataPtrs[0] = Marshal.AllocHGlobal(TP_IMAGEDATA_SIZE);
            RZSBSDK_BUFFERPARAMS tpBufferParams;
            tpBufferParams.PixelType = PIXEL_TYPE.RGB565;
            tpBufferParams.DataSize = TP_IMAGEDATA_SIZE;
            tpBufferParams.PtrData = imageDataPtrs[0];
            bufferParamsPtrs[0] = Marshal.AllocHGlobal(Marshal.SizeOf(tpBufferParams));
            Marshal.StructureToPtr(tpBufferParams, bufferParamsPtrs[0], true);
            bufferMuxes[0] = new Mutex();

            //setup DK image buffers
            for (int i = (int)RZSBSDK_DKTYPE.RZSBSDK_DK_1; i < (int)RZSBSDK_DKTYPE.RZSBSDK_DK_INVALID; i++) {
                imageDataPtrs[i] = Marshal.AllocHGlobal(DK_IMAGEDATA_SIZE);
                RZSBSDK_BUFFERPARAMS dkBufferParams;
                dkBufferParams.PixelType = PIXEL_TYPE.RGB565;
                dkBufferParams.DataSize = DK_IMAGEDATA_SIZE;
                dkBufferParams.PtrData = imageDataPtrs[i];
                bufferParamsPtrs[i] = Marshal.AllocHGlobal(Marshal.SizeOf(dkBufferParams));
                Marshal.StructureToPtr(dkBufferParams, bufferParamsPtrs[i], true);
                bufferMuxes[i] = new Mutex();
            }
        }
        /// <summary>
        /// frees the memory allocated in setupDisplayBuffers()
        /// </summary>
        private static void cleanupDisplayBuffers() {
            Mutex.WaitAll(bufferMuxes);
            for (int i = 0; i < NUMBER_OF_DISPLAYS; i++) {
                Marshal.FreeHGlobal(imageDataPtrs[i]);
                Marshal.FreeHGlobal(bufferParamsPtrs[i]);
                bufferMuxes[i].Dispose();
            }
        }
        #endregion //Display Buffer Setup

        #endregion //start and stop functions

        #region SBFunctions


        private static bool priv_keyboardCaptured = false;
        //C#-style field alternative for CaptureKeyboard()
        //true to capture the keyboard, false to release
        public static bool KeyboardCaptured {
            get { return priv_keyboardCaptured; }
            set {
                if (KeyboardCaptured != value) 
                    CaptureKeyboard(value); 
            }
        }

        //true to capture the keyboard, false to release
        public static void CaptureKeyboard(bool capture) {
            chkStrt();
            SBMux.WaitOne();
            RZSBSDKCheck(NativeMethods.RzSBCaptureKeyboard(capture));
            priv_keyboardCaptured = capture;
            SBMux.ReleaseMutex();
            if (capture) {
                if (OnCaptureKeyboard != null) OnCaptureKeyboard();
            } else {
                if (OnReleaseKeyboard != null) OnReleaseKeyboard();
            }
        }


        #endregion

        #region Exception Classes
        [Serializable]
        public class RZSBSDKNotStartedException : Exception
        {
          public RZSBSDKNotStartedException() { }
          public RZSBSDKNotStartedException( string message ) : base( message ) { }
          public RZSBSDKNotStartedException( string message, Exception inner ) : base( message, inner ) { }
          protected RZSBSDKNotStartedException( 
	        System.Runtime.Serialization.SerializationInfo info, 
	        System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
        }

        [Serializable]
        internal class RZSBSDKException : Exception {

            internal string GetSBSDKResultName() {
                return Enum.GetName(typeof(RZSBSDK_HRESULT), result);
            }

            internal RZSBSDK_HRESULT result {
                get;
                private set;
            }
            internal RZSBSDKException(RZSBSDK_HRESULT res) {
                result = res;
            }
            internal RZSBSDKException(RZSBSDK_HRESULT res, string message) : base(message) {
                result = res;
            }
            internal RZSBSDKException(RZSBSDK_HRESULT res, string message, Exception inner) : base(message, inner) {
                result = res;
            }
            protected RZSBSDKException(
              RZSBSDK_HRESULT res,
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { 
                result = res; 
            }
        }

        #endregion

        #region Exception Checking Util Functions
        /// <summary>
        /// pass in a RZSBSDKAPI call and an exception will be thrown if a bad result is given
        /// </summary>
        /// <param name="res">the return value of the RZSBSDKAPI function</param>
        /// <param name="throwmsg">an optional message to include in the event of a bad result</param>
        private static void RZSBSDKCheck(RZSBSDK_HRESULT res, string throwmsg = "") {
            if (res != RZSBSDK_HRESULT.RZSB_OK) {
                throw new RZSBSDKException(res, throwmsg);
            }
        }

        /// <summary>
        /// Throws an RZSBSDKNotStartedException if the RZSB has not been started
        /// </summary>
        private static void chkStrt() {
            if (!Started) throw new RZSBSDKNotStartedException("RZSB not started!");
        }

        #endregion

        #region SB Event Handlers
        private static RZSBSDK_HRESULT gestureCallback(RZSBSDK_GESTURETYPE gesture, uint dwParameters, ushort wXPos, ushort wYPos, ushort wZPos) {
            //Utils.println(String.Format("Gesture recieved: {0}; data: {1}, x: {2}, y: {3}, z: {4}",
                //Enum.GetName(typeof(RZSBSDK_GESTURETYPE), gesture), dwParameters, wXPos, wYPos, wZPos),
                //ConsoleColor.Blue);
            switch (gesture) {
                case RZSBSDK_GESTURETYPE.RZSBSDK_GESTURE_FLICK: if (OnFlickGesture != null) OnFlickGesture(dwParameters, (FlickDirection)wZPos); break;
                case RZSBSDK_GESTURETYPE.RZSBSDK_GESTURE_MOVE: if (OnMoveGesture != null) OnMoveGesture(wXPos, wYPos); break;
                case RZSBSDK_GESTURETYPE.RZSBSDK_GESTURE_PRESS: if (OnPressGesture != null) OnPressGesture(dwParameters, wXPos, wYPos); break;
                case RZSBSDK_GESTURETYPE.RZSBSDK_GESTURE_RELEASE: if (OnReleaseGesture != null) OnReleaseGesture(dwParameters, wXPos, wYPos); break;
                case RZSBSDK_GESTURETYPE.RZSBSDK_GESTURE_ROTATE: if (OnRotateGesture != null) OnRotateGesture(dwParameters == 1); break;
                case RZSBSDK_GESTURETYPE.RZSBSDK_GESTURE_TAP: if (OnTapGesture != null) OnTapGesture(wXPos, wYPos); break;
                case RZSBSDK_GESTURETYPE.RZSBSDK_GESTURE_ZOOM: if (OnZoomGesture != null) OnZoomGesture(dwParameters == 1); break;
                default: Utils.printf("[SBAPI] An unknown gesture received: {0}", Enum.GetName(typeof(RZSBSDK_GESTURETYPE), gesture)); break;
            }

            return RZSBSDK_HRESULT.RZSB_OK;
        }

        private static RZSBSDK_HRESULT dynamicKeyCallback(RZSBSDK_DKTYPE dynamicKey, RZSBSDK_KEYSTATETYPE dynamicKeyState) {
            //Utils.printf("RzSB dynamic key event recieved: key = {0}; state = {1}", 
            //Enum.GetName(typeof(RZSBSDK_DKTYPE), dynamicKey), 
            //Enum.GetName(typeof(RZSBSDK_KEYSTATETYPE), dynamicKeyState));
            if (OnDynamicKeyEvent != null) OnDynamicKeyEvent((int)dynamicKey, dynamicKeyState==RZSBSDK_KEYSTATETYPE.RZSBSDK_KEYSTATE_DOWN);

            return RZSBSDK_HRESULT.RZSB_OK;
        }

        private static RZSBSDK_HRESULT appEventCallback(RZSBSDK_EVENTTYPETYPE rzEventType, uint dwParam1, uint dwParam2) {
            Utils.printf("[SBAPI] RzSB App event recieved: rzEventType = {0}; dwParam1 = {1}; dwParam2 = {2}", 
                Enum.GetName(typeof(RZSBSDK_EVENTTYPETYPE), rzEventType), 
                dwParam1, 
                dwParam2);
            if (OnAppEvent != null) OnAppEvent((SBEvent)rzEventType);
            if (rzEventType == RZSBSDK_EVENTTYPETYPE.RZSBSDK_EVENT_ACTIVATED && OnActivated != null) OnActivated();
            if (rzEventType == RZSBSDK_EVENTTYPETYPE.RZSBSDK_EVENT_DEACTIVATED && OnDeactivated != null) OnDeactivated();
            if (rzEventType == RZSBSDK_EVENTTYPETYPE.RZSBSDK_EVENT_CLOSE && OnClose != null) OnClose();
            if (rzEventType == RZSBSDK_EVENTTYPETYPE.RZSBSDK_EVENT_EXIT && OnClose != null) OnExit();

            if (rzEventType == RZSBSDK_EVENTTYPETYPE.RZSBSDK_EVENT_CLOSE) {
                Utils.printf("[SBAPI] Close event recieved, shutting down SB connection.");
                Stop();
            }

            return RZSBSDK_HRESULT.RZSB_OK;
        }
        //callback for keyboard events when the keyboard is captured
        private static RZSBSDK_HRESULT keyboardCallback(uint uMsg, UIntPtr wParam, IntPtr lParam) {
            switch (uMsg) {
                case WM_KEYUP: if (OnKeyUp != null) OnKeyUp((VK)wParam, lParam); break;
                case WM_KEYDOWN: if (OnKeyDown != null) OnKeyDown((VK)wParam, lParam); break;
                case WM_CHAR: if (OnKeyTyped != null) OnKeyTyped((char)wParam, lParam); break;
                default: Utils.println("[SBAPI] Invalid Keyboard message (" + uMsg + ")", ConsoleColor.Red); break;
            }
            return RZSBSDK_HRESULT.RZSB_OK;
        }


        #endregion

        #region Misc util functions
        public static Bitmap GenerateBitmapForDK() {
            return new Bitmap(SBAPI.DK_WIDTH, SBAPI.DK_HEIGHT, SBAPI.SB_PIXELFORMAT);
        }

        public static Bitmap GenerateBitmapForTouchpad() {
            return new Bitmap(SBAPI.TP_WIDTH, SBAPI.TP_HEIGHT, SBAPI.SB_PIXELFORMAT);
        }
        #endregion


        //this enum is for the keystrokes from the keyboard events
        //...I think this is/should be built into the C# framework somewhere...
        public enum VK : uint {
            ///<summary>
            ///Left mouse button
            ///</summary>
            LBUTTON = 0x01,
            ///<summary>
            ///Right mouse button
            ///</summary>
            RBUTTON = 0x02,
            ///<summary>
            ///Control-break processing
            ///</summary>
            CANCEL = 0x03,
            ///<summary>
            ///Middle mouse button (three-button mouse)
            ///</summary>
            MBUTTON = 0x04,
            ///<summary>
            ///Windows 2000/XP: X1 mouse button
            ///</summary>
            XBUTTON1 = 0x05,
            ///<summary>
            ///Windows 2000/XP: X2 mouse button
            ///</summary>
            XBUTTON2 = 0x06,
            ///<summary>
            ///BACKSPACE key
            ///</summary>
            BACKSPACE = 0x08,
            ///<summary>
            ///TAB key
            ///</summary>
            TAB = 0x09,
            ///<summary>
            ///CLEAR key
            ///</summary>
            CLEAR = 0x0C,
            ///<summary>
            ///ENTER key
            ///</summary>
            RETURN = 0x0D,
            ///<summary>
            ///SHIFT key
            ///</summary>
            SHIFT = 0x10,
            ///<summary>
            ///CTRL key
            ///</summary>
            CONTROL = 0x11,
            ///<summary>
            ///ALT key
            ///</summary>
            MENU = 0x12,
            ///<summary>
            ///PAUSE key
            ///</summary>
            PAUSE = 0x13,
            ///<summary>
            ///CAPS LOCK key
            ///</summary>
            CAPITAL = 0x14,
            ///<summary>
            ///Input Method Editor (IME) Kana mode
            ///</summary>
            KANA = 0x15,
            ///<summary>
            ///IME Hangul mode
            ///</summary>
            HANGUL = 0x15,
            ///<summary>
            ///IME Junja mode
            ///</summary>
            JUNJA = 0x17,
            ///<summary>
            ///IME final mode
            ///</summary>
            FINAL = 0x18,
            ///<summary>
            ///IME Hanja mode
            ///</summary>
            HANJA = 0x19,
            ///<summary>
            ///IME Kanji mode
            ///</summary>
            KANJI = 0x19,
            ///<summary>
            ///ESC key
            ///</summary>
            ESCAPE = 0x1B,
            ///<summary>
            ///IME convert
            ///</summary>
            CONVERT = 0x1C,
            ///<summary>
            ///IME nonconvert
            ///</summary>
            NONCONVERT = 0x1D,
            ///<summary>
            ///IME accept
            ///</summary>
            ACCEPT = 0x1E,
            ///<summary>
            ///IME mode change request
            ///</summary>
            MODECHANGE = 0x1F,
            ///<summary>
            ///SPACEBAR
            ///</summary>
            SPACE = 0x20,
            ///<summary>
            ///PAGE UP key
            ///</summary>
            PRIOR = 0x21,
            ///<summary>
            ///PAGE DOWN key
            ///</summary>
            NEXT = 0x22,
            ///<summary>
            ///END key
            ///</summary>
            END = 0x23,
            ///<summary>
            ///HOME key
            ///</summary>
            HOME = 0x24,
            ///<summary>
            ///LEFT ARROW key
            ///</summary>
            LEFT = 0x25,
            ///<summary>
            ///UP ARROW key
            ///</summary>
            UP = 0x26,
            ///<summary>
            ///RIGHT ARROW key
            ///</summary>
            RIGHT = 0x27,
            ///<summary>
            ///DOWN ARROW key
            ///</summary>
            DOWN = 0x28,
            ///<summary>
            ///SELECT key
            ///</summary>
            SELECT = 0x29,
            ///<summary>
            ///PRINT key
            ///</summary>
            PRINT = 0x2A,
            ///<summary>
            ///EXECUTE key
            ///</summary>
            EXECUTE = 0x2B,
            ///<summary>
            ///PRINT SCREEN key
            ///</summary>
            SNAPSHOT = 0x2C,
            ///<summary>
            ///INS key
            ///</summary>
            INSERT = 0x2D,
            ///<summary>
            ///DEL key
            ///</summary>
            DELETE = 0x2E,
            ///<summary>
            ///HELP key
            ///</summary>
            HELP = 0x2F,
            ///<summary>
            ///0 key
            ///</summary>
            KEY_0 = 0x30,
            ///<summary>
            ///1 key
            ///</summary>
            KEY_1 = 0x31,
            ///<summary>
            ///2 key
            ///</summary>
            KEY_2 = 0x32,
            ///<summary>
            ///3 key
            ///</summary>
            KEY_3 = 0x33,
            ///<summary>
            ///4 key
            ///</summary>
            KEY_4 = 0x34,
            ///<summary>
            ///5 key
            ///</summary>
            KEY_5 = 0x35,
            ///<summary>
            ///6 key
            ///</summary>
            KEY_6 = 0x36,
            ///<summary>
            ///7 key
            ///</summary>
            KEY_7 = 0x37,
            ///<summary>
            ///8 key
            ///</summary>
            KEY_8 = 0x38,
            ///<summary>
            ///9 key
            ///</summary>
            KEY_9 = 0x39,
            ///<summary>
            ///A key
            ///</summary>
            KEY_A = 0x41,
            ///<summary>
            ///B key
            ///</summary>
            KEY_B = 0x42,
            ///<summary>
            ///C key
            ///</summary>
            KEY_C = 0x43,
            ///<summary>
            ///D key
            ///</summary>
            KEY_D = 0x44,
            ///<summary>
            ///E key
            ///</summary>
            KEY_E = 0x45,
            ///<summary>
            ///F key
            ///</summary>
            KEY_F = 0x46,
            ///<summary>
            ///G key
            ///</summary>
            KEY_G = 0x47,
            ///<summary>
            ///H key
            ///</summary>
            KEY_H = 0x48,
            ///<summary>
            ///I key
            ///</summary>
            KEY_I = 0x49,
            ///<summary>
            ///J key
            ///</summary>
            KEY_J = 0x4A,
            ///<summary>
            ///K key
            ///</summary>
            KEY_K = 0x4B,
            ///<summary>
            ///L key
            ///</summary>
            KEY_L = 0x4C,
            ///<summary>
            ///M key
            ///</summary>
            KEY_M = 0x4D,
            ///<summary>
            ///N key
            ///</summary>
            KEY_N = 0x4E,
            ///<summary>
            ///O key
            ///</summary>
            KEY_O = 0x4F,
            ///<summary>
            ///P key
            ///</summary>
            KEY_P = 0x50,
            ///<summary>
            ///Q key
            ///</summary>
            KEY_Q = 0x51,
            ///<summary>
            ///R key
            ///</summary>
            KEY_R = 0x52,
            ///<summary>
            ///S key
            ///</summary>
            KEY_S = 0x53,
            ///<summary>
            ///T key
            ///</summary>
            KEY_T = 0x54,
            ///<summary>
            ///U key
            ///</summary>
            KEY_U = 0x55,
            ///<summary>
            ///V key
            ///</summary>
            KEY_V = 0x56,
            ///<summary>
            ///W key
            ///</summary>
            KEY_W = 0x57,
            ///<summary>
            ///X key
            ///</summary>
            KEY_X = 0x58,
            ///<summary>
            ///Y key
            ///</summary>
            KEY_Y = 0x59,
            ///<summary>
            ///Z key
            ///</summary>
            KEY_Z = 0x5A,
            ///<summary>
            ///Left Windows key (Microsoft Natural keyboard)
            ///</summary>
            LWIN = 0x5B,
            ///<summary>
            ///Right Windows key (Natural keyboard)
            ///</summary>
            RWIN = 0x5C,
            ///<summary>
            ///Applications key (Natural keyboard)
            ///</summary>
            APPS = 0x5D,
            ///<summary>
            ///Computer Sleep key
            ///</summary>
            SLEEP = 0x5F,
            ///<summary>
            ///Numeric keypad 0 key
            ///</summary>
            NUMPAD0 = 0x60,
            ///<summary>
            ///Numeric keypad 1 key
            ///</summary>
            NUMPAD1 = 0x61,
            ///<summary>
            ///Numeric keypad 2 key
            ///</summary>
            NUMPAD2 = 0x62,
            ///<summary>
            ///Numeric keypad 3 key
            ///</summary>
            NUMPAD3 = 0x63,
            ///<summary>
            ///Numeric keypad 4 key
            ///</summary>
            NUMPAD4 = 0x64,
            ///<summary>
            ///Numeric keypad 5 key
            ///</summary>
            NUMPAD5 = 0x65,
            ///<summary>
            ///Numeric keypad 6 key
            ///</summary>
            NUMPAD6 = 0x66,
            ///<summary>
            ///Numeric keypad 7 key
            ///</summary>
            NUMPAD7 = 0x67,
            ///<summary>
            ///Numeric keypad 8 key
            ///</summary>
            NUMPAD8 = 0x68,
            ///<summary>
            ///Numeric keypad 9 key
            ///</summary>
            NUMPAD9 = 0x69,
            ///<summary>
            ///Multiply key
            ///</summary>
            MULTIPLY = 0x6A,
            ///<summary>
            ///Add key
            ///</summary>
            ADD = 0x6B,
            ///<summary>
            ///Separator key
            ///</summary>
            SEPARATOR = 0x6C,
            ///<summary>
            ///Subtract key
            ///</summary>
            SUBTRACT = 0x6D,
            ///<summary>
            ///Decimal key
            ///</summary>
            DECIMAL = 0x6E,
            ///<summary>
            ///Divide key
            ///</summary>
            DIVIDE = 0x6F,
            ///<summary>
            ///F1 key
            ///</summary>
            F1 = 0x70,
            ///<summary>
            ///F2 key
            ///</summary>
            F2 = 0x71,
            ///<summary>
            ///F3 key
            ///</summary>
            F3 = 0x72,
            ///<summary>
            ///F4 key
            ///</summary>
            F4 = 0x73,
            ///<summary>
            ///F5 key
            ///</summary>
            F5 = 0x74,
            ///<summary>
            ///F6 key
            ///</summary>
            F6 = 0x75,
            ///<summary>
            ///F7 key
            ///</summary>
            F7 = 0x76,
            ///<summary>
            ///F8 key
            ///</summary>
            F8 = 0x77,
            ///<summary>
            ///F9 key
            ///</summary>
            F9 = 0x78,
            ///<summary>
            ///F10 key
            ///</summary>
            F10 = 0x79,
            ///<summary>
            ///F11 key
            ///</summary>
            F11 = 0x7A,
            ///<summary>
            ///F12 key
            ///</summary>
            F12 = 0x7B,
            ///<summary>
            ///F13 key
            ///</summary>
            F13 = 0x7C,
            ///<summary>
            ///F14 key
            ///</summary>
            F14 = 0x7D,
            ///<summary>
            ///F15 key
            ///</summary>
            F15 = 0x7E,
            ///<summary>
            ///F16 key
            ///</summary>
            F16 = 0x7F,
            ///<summary>
            ///F17 key  
            ///</summary>
            F17 = 0x80,
            ///<summary>
            ///F18 key  
            ///</summary>
            F18 = 0x81,
            ///<summary>
            ///F19 key  
            ///</summary>
            F19 = 0x82,
            ///<summary>
            ///F20 key  
            ///</summary>
            F20 = 0x83,
            ///<summary>
            ///F21 key  
            ///</summary>
            F21 = 0x84,
            ///<summary>
            ///F22 key, (PPC only) Key used to lock device.
            ///</summary>
            F22 = 0x85,
            ///<summary>
            ///F23 key  
            ///</summary>
            F23 = 0x86,
            ///<summary>
            ///F24 key  
            ///</summary>
            F24 = 0x87,
            ///<summary>
            ///NUM LOCK key
            ///</summary>
            NUMLOCK = 0x90,
            ///<summary>
            ///SCROLL LOCK key
            ///</summary>
            SCROLL = 0x91,
            ///<summary>
            ///Left SHIFT key
            ///</summary>
            LSHIFT = 0xA0,
            ///<summary>
            ///Right SHIFT key
            ///</summary>
            RSHIFT = 0xA1,
            ///<summary>
            ///Left CONTROL key
            ///</summary>
            LCONTROL = 0xA2,
            ///<summary>
            ///Right CONTROL key
            ///</summary>
            RCONTROL = 0xA3,
            ///<summary>
            ///Left MENU key
            ///</summary>
            LMENU = 0xA4,
            ///<summary>
            ///Right MENU key
            ///</summary>
            RMENU = 0xA5,
            ///<summary>
            ///Windows 2000/XP: Browser Back key
            ///</summary>
            BROWSER_BACK = 0xA6,
            ///<summary>
            ///Windows 2000/XP: Browser Forward key
            ///</summary>
            BROWSER_FORWARD = 0xA7,
            ///<summary>
            ///Windows 2000/XP: Browser Refresh key
            ///</summary>
            BROWSER_REFRESH = 0xA8,
            ///<summary>
            ///Windows 2000/XP: Browser Stop key
            ///</summary>
            BROWSER_STOP = 0xA9,
            ///<summary>
            ///Windows 2000/XP: Browser Search key
            ///</summary>
            BROWSER_SEARCH = 0xAA,
            ///<summary>
            ///Windows 2000/XP: Browser Favorites key
            ///</summary>
            BROWSER_FAVORITES = 0xAB,
            ///<summary>
            ///Windows 2000/XP: Browser Start and Home key
            ///</summary>
            BROWSER_HOME = 0xAC,
            ///<summary>
            ///Windows 2000/XP: Volume Mute key
            ///</summary>
            VOLUME_MUTE = 0xAD,
            ///<summary>
            ///Windows 2000/XP: Volume Down key
            ///</summary>
            VOLUME_DOWN = 0xAE,
            ///<summary>
            ///Windows 2000/XP: Volume Up key
            ///</summary>
            VOLUME_UP = 0xAF,
            ///<summary>
            ///Windows 2000/XP: Next Track key
            ///</summary>
            MEDIA_NEXT_TRACK = 0xB0,
            ///<summary>
            ///Windows 2000/XP: Previous Track key
            ///</summary>
            MEDIA_PREV_TRACK = 0xB1,
            ///<summary>
            ///Windows 2000/XP: Stop Media key
            ///</summary>
            MEDIA_STOP = 0xB2,
            ///<summary>
            ///Windows 2000/XP: Play/Pause Media key
            ///</summary>
            MEDIA_PLAY_PAUSE = 0xB3,
            ///<summary>
            ///Windows 2000/XP: Start Mail key
            ///</summary>
            LAUNCH_MAIL = 0xB4,
            ///<summary>
            ///Windows 2000/XP: Select Media key
            ///</summary>
            LAUNCH_MEDIA_SELECT = 0xB5,
            ///<summary>
            ///Windows 2000/XP: Start Application 1 key
            ///</summary>
            LAUNCH_APP1 = 0xB6,
            ///<summary>
            ///Windows 2000/XP: Start Application 2 key
            ///</summary>
            LAUNCH_APP2 = 0xB7,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///The ;: key on standard US keyboards
            ///</summary>
            OEM_1 = 0xBA,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '+' key
            ///</summary>
            OEM_PLUS = 0xBB,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the ',' key
            ///</summary>
            OEM_COMMA = 0xBC,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '-' key
            ///</summary>
            OEM_MINUS = 0xBD,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '.' key
            ///</summary>
            OEM_PERIOD = 0xBE,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_2 = 0xBF,
            ///<summary>
            ///The `~ key on standard US keyboards
            ///</summary>
            TILDE = 0xC0,
            ///<summary>
            ///the {[ key
            ///</summary>
            OPEN_BRACKET = 0xDB,
            ///<summary>
            /// the \| key
            ///</summary>
            BACKSLASH = 0xDC,
            ///<summary>
            ///the ]} key
            ///</summary>
            CLOSE_BRACKET = 0xDD,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_7 = 0xDE,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_8 = 0xDF,
            ///<summary>
            ///Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
            ///</summary>
            OEM_102 = 0xE2,
            ///<summary>
            ///Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
            ///</summary>
            PROCESSKEY = 0xE5,
            ///<summary>
            ///Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information, see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
            ///</summary>
            PACKET = 0xE7,
            ///<summary>
            ///Attn key
            ///</summary>
            ATTN = 0xF6,
            ///<summary>
            ///CrSel key
            ///</summary>
            CRSEL = 0xF7,
            ///<summary>
            ///ExSel key
            ///</summary>
            EXSEL = 0xF8,
            ///<summary>
            ///Erase EOF key
            ///</summary>
            EREOF = 0xF9,
            ///<summary>
            ///Play key
            ///</summary>
            PLAY = 0xFA,
            ///<summary>
            ///Zoom key
            ///</summary>
            ZOOM = 0xFB,
            ///<summary>
            ///Reserved
            ///</summary>
            NONAME = 0xFC,
            ///<summary>
            ///PA1 key
            ///</summary>
            PA1 = 0xFD,
            ///<summary>
            ///Clear key
            ///</summary>
            OEM_CLEAR = 0xFE
        }
    }
}
