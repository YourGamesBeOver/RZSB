﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;
using RZSB;

namespace RZSB.TouchpadGraphics {
    public class TPRootPanel : TPPanel {
        private Bitmap bmp = SBAPI.GenerateBitmapForTouchpad();
        private static Rectangle ST_BOUNDS = new Rectangle(0, 0, SBAPI.TP_WIDTH, SBAPI.TP_HEIGHT);

        private Mutex queueMux = new Mutex();
        private Queue<TPEvent> eventQueue = new Queue<TPEvent>();

        private AutoResetEvent queueReadyEvent = new AutoResetEvent(false);

        private Thread renderThread;


        public override Point Position {
            get {
                return ST_BOUNDS.Location;
            }
        }

        public override Size Size {
            get {
                return ST_BOUNDS.Size;
            }
        }

        public override TPPanel Parent {
            get {
                return null;
            }
            internal set {
                throw new Exception("TPRootPanel Must always be the root TPComponent!");
            }
        }

        public TPRootPanel() {
            if (!SBAPI.Started) SBAPI.Start();
            OnEnable += TPRootPanel_OnEnable;
            SBAPI.OnPressGesture += SBAPI_OnPressGesture;
            SBAPI.OnReleaseGesture += SBAPI_OnReleaseGesture;
            SBAPI.OnTapGesture += SBAPI_OnTapGesture;
            SBAPI.OnActivated += SBAPI_OnActivated;
            SBAPI.OnMoveGesture += SBAPI_OnMoveGesture;
            renderThread = new Thread(new ThreadStart(RedrawThreadStart));
            renderThread.Start();
        }

        void SBAPI_OnMoveGesture(ushort xPos, ushort yPos) {
            //FingerOver(xPos, yPos);
            addEvent(TPEventType.MOVE, xPos, yPos);
        }

        void SBAPI_OnActivated() {
            RequestTotalRedraw();
        }

        void SBAPI_OnTapGesture(ushort xPos, ushort yPos) {
            //Tapped(xPos, yPos);
            addEvent(TPEventType.TAP, xPos, yPos);
        }

        void SBAPI_OnReleaseGesture(uint touchpoints, ushort xPos, ushort yPos) {
            //Released(touchpoints, xPos, yPos);
            addEvent(TPEventType.RELEASE, xPos, yPos, touchpoints);
        }

        void SBAPI_OnPressGesture(uint touchpoints, ushort xPos, ushort yPos) {
            //Pressed(touchpoints, xPos, yPos);
            addEvent(TPEventType.PRESS, xPos, yPos, touchpoints);
        }

        void TPRootPanel_OnEnable(TPComponent sender) {
            RequestTotalRedraw();
        }

        protected override void RequestTotalRedraw() {
            addEvent(TPEventType.NONE, 0, 0);
        }

        private void priv_RequestTotalRedraw() {
            Graphics g = Graphics.FromImage(bmp);
            Draw(ref g);
            RedrawAllChildren(ref g);
            g.Dispose();

            if (Enabled) {
                SBAPI.WriteBitmapImageToSB(SBDisplays.TRACKPAD, bmp);
            }
        }

        private void addEvent(TPEventType type, ushort x, ushort y, uint tps = 0) {
            queueMux.WaitOne();
            eventQueue.Enqueue(new TPEvent(type, x, y, tps));
            queueMux.ReleaseMutex();
            queueReadyEvent.Set();
        }

        private struct TPEvent{
            internal TPEventType eventType;
            internal uint touchpoints;
            internal ushort xPos;
            internal ushort yPos;
            internal TPEvent(TPEventType type, ushort x, ushort y, uint tps = 0){
                eventType = type;
                xPos = x;
                yPos = y;
                touchpoints = tps;
            }
        }

        private enum TPEventType{
            NONE, TAP, PRESS, RELEASE, MOVE
        }

        private void RedrawThreadStart() {
            Queue<TPEvent> privEventQueue = new Queue<TPEvent>();
            while (true) {
                queueReadyEvent.WaitOne();//wait for work
                queueMux.WaitOne();
                foreach(TPEvent e in eventQueue){
                    privEventQueue.Enqueue(e);
                }
                eventQueue.Clear();
                queueMux.ReleaseMutex();
                foreach (TPEvent e in privEventQueue) {
                    switch (e.eventType) {
                        case TPEventType.MOVE: FingerOver((int)e.xPos, (int)e.yPos); break;
                        case TPEventType.PRESS: Pressed(e.touchpoints, (int)e.xPos, (int)e.yPos); break;
                        case TPEventType.RELEASE: Released(e.touchpoints, (int)e.xPos, (int)e.yPos); break;
                        case TPEventType.TAP: Tapped((int)e.xPos, (int)e.yPos); break;
                        default: break;
                    }
                }
                privEventQueue.Clear();
                priv_RequestTotalRedraw();
            }
        }

        protected override void DisposeManagedResources() {
            base.DisposeManagedResources();
            renderThread.Abort();
        }
    }
}
