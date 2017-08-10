﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DWSIM.Drawing.SkiaSharp;
using DWSIM.UI.Controls;
using SkiaSharp;

namespace DWSIM.UI.Desktop.GTK
{
    public class FlowsheetSurfaceControlHandler : Eto.GtkSharp.Forms.GtkControl<Gtk.EventBox, FlowsheetSurfaceControl, FlowsheetSurfaceControl.ICallback>, FlowsheetSurfaceControl.IFlowsheetSurface
    {
        public FlowsheetSurfaceControlHandler()
        {
            this.Control = new FlowsheetSurface_GTK();
        }

        public override Eto.Drawing.Color BackgroundColor
        {
            get
            {
                return Eto.Drawing.Colors.White;
            }
            set
            {
                return;
            }
        }

        public GraphicsSurface FlowsheetSurface
        {
            get
            {
                return ((FlowsheetSurface_GTK)this.Control).fsurface;
            }
            set
            {
                ((FlowsheetSurface_GTK)this.Control).fsurface = value;
            }
        }

        public DWSIM.UI.Desktop.Shared.Flowsheet FlowsheetObject
        {
            get
            {
                return ((FlowsheetSurface_GTK)this.Control).fbase;
            }
            set
            {
                ((FlowsheetSurface_GTK)this.Control).fbase = value;
            }
        }

    }

    public class FlowsheetSurface_GTK : Gtk.EventBox
    {

        public GraphicsSurface fsurface;
        public DWSIM.UI.Desktop.Shared.Flowsheet fbase;

        private float _lastTouchX;
        private float _lastTouchY;

        public FlowsheetSurface_GTK()
        {
            this.AddEvents((int)Gdk.EventMask.PointerMotionMask);
            this.ButtonPressEvent += FlowsheetSurface_GTK_ButtonPressEvent;
            this.ButtonReleaseEvent += FlowsheetSurface_GTK_ButtonReleaseEvent;
            this.MotionNotifyEvent += FlowsheetSurface_GTK_MotionNotifyEvent;
            this.ScrollEvent += FlowsheetSurface_GTK_ScrollEvent;
        }

        void FlowsheetSurface_GTK_ScrollEvent(object o, Gtk.ScrollEventArgs args)
        {
            if (args.Event.Direction == Gdk.ScrollDirection.Down)
            {
                fsurface.Zoom += -5 / 100f;
            }
            else
            {
                fsurface.Zoom += 5 / 100f;
            }
            this.QueueDraw();
        }

        void FlowsheetSurface_GTK_MotionNotifyEvent(object o, Gtk.MotionNotifyEventArgs args)
        {
            float x = (int)args.Event.X;
            float y = (int)args.Event.Y;
            _lastTouchX = x;
            _lastTouchY = y;
            fsurface.InputMove((int)_lastTouchX, (int)_lastTouchY);
            this.QueueDraw();
        }

        void FlowsheetSurface_GTK_ButtonReleaseEvent(object o, Gtk.ButtonReleaseEventArgs args)
        {
            fsurface.InputRelease();
            this.QueueDraw();
        }

        void FlowsheetSurface_GTK_ButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
        {
            if (args.Event.Type == Gdk.EventType.TwoButtonPress)
            {
                //if (args.Event.State == Gdk.ModifierType.ShiftMask)
                //{
                //    fsurface.Zoom = 1.0f;
                //}
                //else {
                //    fsurface.ZoomAll((int)this.Allocation.Width, (int)this.Allocation.Height);
                //}
            }
            else
            {
                _lastTouchX = (int)args.Event.X;
                _lastTouchY = (int)args.Event.Y;
                fsurface.InputPress((int)_lastTouchX, (int)_lastTouchY);
            }
            this.QueueDraw();

        }

        protected override bool OnExposeEvent(Gdk.EventExpose evnt)
        {

            var rect = Allocation;

            if (rect.Width > 0 && rect.Height > 0)
            {
                var area = evnt.Area;
                using (Cairo.Context cr = Gdk.CairoHelper.Create(base.GdkWindow))
                {
                    using (var bitmap = new SKBitmap(rect.Width, rect.Height, SKColorType.Bgra8888, SKAlphaType.Premul))
                    {
                        IntPtr len;
                        using (var skSurface = SKSurface.Create(bitmap.Info.Width, bitmap.Info.Height, SKColorType.Bgra8888, SKAlphaType.Premul, bitmap.GetPixels(out len), bitmap.Info.RowBytes))
                        {
                            if (fsurface != null) fsurface.UpdateSurface(skSurface);
                            skSurface.Canvas.Flush();
                            using (Cairo.Surface surface = new Cairo.ImageSurface(bitmap.GetPixels(out len), Cairo.Format.Argb32, bitmap.Width, bitmap.Height, bitmap.Width * 4))
                            {
                                surface.MarkDirty();
                                cr.SetSourceSurface(surface, 0, 0);
                                cr.Paint();
                            }
                        }
                    }
                }

            }

            return true;
        }

    }

}
