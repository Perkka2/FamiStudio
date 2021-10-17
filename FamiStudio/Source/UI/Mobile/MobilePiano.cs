﻿using System;
using System.Diagnostics;

using Color     = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

using RenderBitmapAtlas = FamiStudio.GLBitmapAtlas;
using RenderBrush       = FamiStudio.GLBrush;
using RenderControl     = FamiStudio.GLControl;
using RenderGraphics    = FamiStudio.GLGraphics;
using RenderFont        = FamiStudio.GLFont;

namespace FamiStudio
{
    public class MobilePiano : RenderControl
    {
        const int NumOctaves = 8;
        const int NumNotes   = NumOctaves * 12;

        const int DefaultButtonSize    = 135;
        const int DefaultIconPos       = 12;
        const int DefaultWhiteKeySizeX = 120;
        const int DefaultBlackKeySizeX = 96;

        const float MinZoom = 0.5f;
        const float MaxZoom = 2.0f;

        enum CaptureOperation
        {
            None,
            MobilePan,
            MobileZoom,
            PlayPiano
        }

        private enum ButtonImageIndices
        {
            MobilePianoDrag,
            MobilePianoRest,
            Count
        };

        private readonly string[] ButtonImageNames = new string[]
        {
            "MobilePianoDrag",
            "MobilePianoRest"
        };

        private int whiteKeySizeX;
        private int blackKeySizeX;
        private int octaveSizeX;
        private int virtualSizeX;

        RenderBrush whiteKeyBrush;
        RenderBrush blackKeyBrush;
        RenderBrush whiteKeyPressedBrush;
        RenderBrush blackKeyPressedBrush;
        RenderBitmapAtlas bmpButtonAtlas;

        private int scrollX = -1;
        private int playAbsNote = -1;
        private int highlightAbsNote = Note.NoteInvalid;
        private int lastX;
        private int lastY;
        private int layoutSize;
        private float zoom = 1.0f;
        private float flingVelX;
        private bool canFling = false;
        private CaptureOperation captureOperation = CaptureOperation.None;
        
        public int LayoutSize => layoutSize;

        protected override void OnRenderInitialized(RenderGraphics g)
        {
            Debug.Assert((int)ButtonImageIndices.Count == ButtonImageNames.Length);

            var screenSize = PlatformUtils.GetScreenResolution();
            layoutSize = Math.Min(screenSize.Width, screenSize.Height) / 4;

            bmpButtonAtlas       = g.CreateBitmapAtlasFromResources(ButtonImageNames);
            whiteKeyBrush        = g.CreateVerticalGradientBrush(0, layoutSize, Theme.LightGreyFillColor1, Theme.LightGreyFillColor2);
            blackKeyBrush        = g.CreateVerticalGradientBrush(0, layoutSize, Theme.DarkGreyFillColor1,  Theme.DarkGreyFillColor2);
            whiteKeyPressedBrush = g.CreateVerticalGradientBrush(0, layoutSize, Theme.Darken(Theme.LightGreyFillColor1), Theme.Darken(Theme.LightGreyFillColor2));
            blackKeyPressedBrush = g.CreateVerticalGradientBrush(0, layoutSize, Theme.Lighten(Theme.DarkGreyFillColor1), Theme.Lighten(Theme.DarkGreyFillColor2));
        }
        
        private void UpdateRenderCoords()
        {
            var screenSize = PlatformUtils.GetScreenResolution();
            var scale = Math.Min(screenSize.Width, screenSize.Height) / 1080.0f;

            whiteKeySizeX = ScaleCustom(DefaultWhiteKeySizeX, scale * zoom);
            blackKeySizeX = ScaleCustom(DefaultBlackKeySizeX, scale * zoom);
            octaveSizeX   = 7 * whiteKeySizeX;
            virtualSizeX  = octaveSizeX * NumOctaves;

            // Center the piano initially.
            if (scrollX < 0)
                scrollX = (virtualSizeX - Width) / 2;
        }

        protected override void OnRenderTerminated()
        {
            Utils.DisposeAndNullify(ref bmpButtonAtlas);
            Utils.DisposeAndNullify(ref whiteKeyBrush);
            Utils.DisposeAndNullify(ref blackKeyBrush);
            Utils.DisposeAndNullify(ref whiteKeyPressedBrush);
            Utils.DisposeAndNullify(ref blackKeyPressedBrush);
        }

        protected override void OnResize(EventArgs e)
        {
            UpdateRenderCoords();
            ClampScroll();
        }

        private void TickFling(float delta)
        {
            if (flingVelX != 0.0f)
            {
                var deltaPixel = (int)Math.Round(flingVelX * delta);
                if (deltaPixel != 0 && DoScroll(-deltaPixel))
                    flingVelX *= (float)Math.Exp(delta * -4.5f);
                else
                    flingVelX = 0.0f;
            }
        }

        public void Tick(float delta)
        {
            TickFling(delta);
        }

        public void HighlightPianoNote(int note)
        {
            if (note != highlightAbsNote)
            {
                highlightAbsNote = note;
                MarkDirty();
            }
        }

        private bool IsBlackKey(int key)
        {
            return key == 1 || key == 3 || key == 6 || key == 8 || key == 10;
        }

        private Rectangle GetKeyRectangle(int octave, int key)
        {
            var blackKey = IsBlackKey(key);

            key = key <= 4 ? key / 2 : 3 + (key - 5) / 2;

            if (blackKey)
                return new Rectangle(octaveSizeX * octave + key * whiteKeySizeX + whiteKeySizeX - blackKeySizeX / 2 - scrollX, 0, blackKeySizeX, Height / 2);
            else
                return new Rectangle(octaveSizeX * octave + key * whiteKeySizeX - scrollX, 0, whiteKeySizeX, Height);
        }

        private Rectangle GetPanRectangle(int octave, int idx)
        {
            if (octave == NumOctaves - 1 && idx == 1)
                return Rectangle.Empty;

            var r0 = GetKeyRectangle(octave,       idx == 0 ? 3 : 10);
            var r1 = GetKeyRectangle(octave + idx, idx == 0 ? 6 : 1);

            return new Rectangle(r0.Right, 0, r1.Left - r0.Right, Height / 2);
        }

        private bool GetDPCMKeyColor(int note, ref Color color)
        {
            if (App.SelectedChannel.Type == ChannelType.Dpcm)
            {
                var mapping = App.Project.GetDPCMMapping(note);
                if (mapping != null)
                {
                    color = mapping.Sample.Color;
                    return true;
                }
            }

            return false;
        }

        protected void RenderDebug(RenderGraphics g)
        {
#if DEBUG
            if (PlatformUtils.IsMobile)
            {
                var c = g.CreateCommandList();
                c.FillRectangle(lastX - 30, lastY - 30, lastX + 30, lastY + 30, ThemeResources.WhiteBrush);
                g.DrawCommandList(c);
            }
#endif
        }

        protected void RenderPiano(RenderGraphics g)
        {
            int minVisibleOctave = Utils.Clamp((int)Math.Floor(scrollX / (float)octaveSizeX), 0, NumOctaves);
            int maxVisibleOctave = Utils.Clamp((int)Math.Ceiling((scrollX + Width) / (float)octaveSizeX), 0, NumOctaves);

            var cb = g.CreateCommandList();
            var cp = g.CreateCommandList();
           
            // Background (white keys)
            cb.FillRectangle(0, 0, Width, Height, whiteKeyBrush);

            // Highlighted note.
            var playOctave = Note.IsMusicalNote(highlightAbsNote) ? (highlightAbsNote - 1) / 12 : -1;
            var playNote   = Note.IsMusicalNote(highlightAbsNote) ? (highlightAbsNote - 1) % 12 : -1;
            if (playNote >= 0 && !IsBlackKey(playNote))
                cp.FillRectangle(GetKeyRectangle(playOctave, playNote), whiteKeyPressedBrush);

            var color = Color.Empty;

            // Early pass for DPCM white keys
            for (int i = minVisibleOctave; i < maxVisibleOctave; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    if (!IsBlackKey(j) && GetDPCMKeyColor(i * 12 + j + 1, ref color))
                        cp.FillRectangle(GetKeyRectangle(i, j), g.GetVerticalGradientBrush(Theme.Darken(color, 20), color, Height));
                }
            }

            // Black keys
            for (int i = minVisibleOctave; i < maxVisibleOctave; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    if (IsBlackKey(j))
                    {
                        var brush = playOctave == i && playNote == j ? blackKeyPressedBrush : blackKeyBrush;
                        if (GetDPCMKeyColor(i * 12 + j + 1, ref color))
                            brush = g.GetVerticalGradientBrush(Theme.Darken(color, 40), Theme.Darken(color, 20), Height / 2);
                        cp.FillRectangle(GetKeyRectangle(i, j), brush);
                    }
                }
            }

            // Lines between white keys
            for (int i = minVisibleOctave; i < maxVisibleOctave; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    if (!IsBlackKey(j))
                    {
                        var groupStart = j == 0 || j == 5;
                        var x = GetKeyRectangle(i, j).X;
                        var y = groupStart ? 0 : Height / 2;
                        var brush = groupStart ? ThemeResources.BlackBrush : ThemeResources.DarkGreyFillBrush2;
                        cp.DrawLine(x, y, x, Height, brush);
                    }
                }
            }

            // Top line
            cp.DrawLine(0, 0, Width, 0, ThemeResources.BlackBrush);

            // Octave labels
            for (int i = minVisibleOctave; i < maxVisibleOctave; i++)
            {
                var r = GetKeyRectangle(i, 0);
                cp.DrawText("C" + i, ThemeResources.FontSmall, r.X, r.Y, ThemeResources.BlackBrush, RenderTextFlags.BottomCenter, r.Width, r.Height - ThemeResources.FontSmall.Size);
            }

            // Drag images
            for (int i = minVisibleOctave; i < maxVisibleOctave; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    var r = GetPanRectangle(i, j);
                    if (!r.IsEmpty)
                    {
                        var size = bmpButtonAtlas.GetElementSize((int)ButtonImageIndices.MobilePianoDrag);
                        var scale = Math.Min(r.Width, r.Height) / (float)Math.Min(size.Width, size.Height);
                        var posX = r.X + r.Width / 2 - (int)(size.Width * scale / 2);
                        var posY = r.Height / 2 - (int)(size.Height * scale / 2);
                        var imageIndex = App.IsRecording && j == 1 ? (int)ButtonImageIndices.MobilePianoRest : (int)ButtonImageIndices.MobilePianoDrag;
                        cp.DrawBitmapAtlas(bmpButtonAtlas, imageIndex, posX, posY, 0.25f, scale, Color.Black);
                    }
                }
            }

            //if ((editMode == EditionMode.Channel || editMode == EditionMode.DPCMMapping) && ThemeResources.FontSmall.Size < noteSizeY)
            //    r.cp.DrawText("C" + i, ThemeResources.FontSmall, r.g.WindowScaling, octaveBaseX - noteSizeY + 1, ThemeResources.BlackBrush, RenderTextFlags.Middle, whiteKeySizeX - r.g.WindowScaling * 2, noteSizeY - 1);
            //if ((i == playOctave && j == playNote) || (draggingNote && (i == dragOctave && j == dragNote)))
            //    r.cp.FillRectangle(GetKeyRectangle(i, j), blackKeyPressedBrush);

            g.DrawCommandList(cb);
            g.DrawCommandList(cp, new Rectangle(0, 0, Width, Height));
        }

        protected override void OnRender(RenderGraphics g)
        {
            RenderPiano(g); 
            RenderDebug(g);
        }

        private void ZoomAtLocation(int x, float scale)
        {
            if (scale == 1.0f)
                return;

            var absoluteX = x + scrollX;
            var prevNoteSizeX = whiteKeySizeX;

            zoom *= scale;
            zoom = Utils.Clamp(zoom, MinZoom, MaxZoom);

            // This will update the noteSizeX.
            UpdateRenderCoords();

            absoluteX = (int)Math.Round(absoluteX * (whiteKeySizeX / (double)prevNoteSizeX));
            scrollX = absoluteX - x;

            ClampScroll();
            MarkDirty();
        }

        private bool ClampScroll()
        {
            var minScrollX = 0;
            var maxScrollX = Math.Max(virtualSizeX - Width, 0);

            var scrolled = true;
            if (scrollX < minScrollX) { scrollX = minScrollX; scrolled = false; }
            if (scrollX > maxScrollX) { scrollX = maxScrollX; scrolled = false; }
            return scrolled;
        }

        private bool DoScroll(int deltaX)
        {
            scrollX += deltaX;
            MarkDirty();
            return ClampScroll();
        }

        protected int GetPianoNote(int x, int y)
        {
            for (int i = 0; i < NumOctaves; i++)
            {
                for (int j = 0; j < 12 && i * 12 + j < NumNotes; j++)
                {
                    if (IsBlackKey(j) && GetKeyRectangle(i, j).Contains(x, y))
                        return i * 12 + j + 1;
                }
                for (int j = 0; j < 12 && i * 12 + j < NumNotes; j++)
                {
                    if (!IsBlackKey(j) && GetKeyRectangle(i, j).Contains(x, y))
                        return i * 12 + j + 1;
                }
            }

            return -1;
        }

        protected void PlayPiano(int x, int y)
        {
            var note = GetPianoNote(x, y);
            if (note >= 0)
            {
                if (note != playAbsNote)
                {
                    playAbsNote = note;
                    App.PlayInstrumentNote(playAbsNote, true, true);
                    PlatformUtils.VibrateTick();
                    MarkDirty();
                }
            }
        }

        private void EndPlayPiano()
        {
            App.StopOrReleaseIntrumentNote(false);
            playAbsNote = -1;
        }

        private void StartCaptureOperation(int x, int y, CaptureOperation op)
        {
            lastX = x;
            lastY = y;
            captureOperation = op;
            Capture = true;
            canFling = false;
        }

        private void UpdateCaptureOperation(int x, int y, float scale = 1.0f)
        {
            switch (captureOperation)
            {
                case CaptureOperation.MobilePan:
                    DoScroll(lastX - x);
                    break;
                case CaptureOperation.PlayPiano:
                    PlayPiano(x, y);
                    break;
                case CaptureOperation.MobileZoom:
                    ZoomAtLocation(x, scale);
                    DoScroll(lastX - x);
                    break;
            }
        }

        private void EndCaptureOperation(int x, int y)
        {
            switch (captureOperation)
            {
                case CaptureOperation.PlayPiano:
                    EndPlayPiano();
                    break;
                case CaptureOperation.MobilePan:
                case CaptureOperation.MobileZoom:
                    canFling = true;
                    break;
            }

            Capture = false;
            captureOperation = CaptureOperation.None;
            MarkDirty();
        }

        protected override void OnTouchUp(int x, int y)
        {
            EndCaptureOperation(x, y);
        }

        private bool IsPointInPanRectangle(int x, int y)
        {
            for (int i = 0; i < NumOctaves; i++)
            {
                var maxIdx = App.IsRecording ? 1 : 2;
                for (int j = 0; j < maxIdx; j++)
                {
                    if (GetPanRectangle(i, j).Contains(x, y))
                        return true;
                }
            }

            return false;
        }

        private bool IsPointInRestRectangle(int x, int y)
        {
            if (App.IsRecording)
            {
                for (int i = 0; i < NumOctaves; i++)
                {
                    if (GetPanRectangle(i, 1).Contains(x, y))
                        return true;
                }
            }

            return false;
        }

        protected override void OnTouchDown(int x, int y)
        {
            Debug.Assert(captureOperation == CaptureOperation.None);

            flingVelX = 0;
            lastX = x;
            lastY = y;

            if (IsPointInPanRectangle(x, y))
                StartCaptureOperation(x, y, CaptureOperation.MobilePan);
            else if (IsPointInRestRectangle(x, y))
                App.AdvanceRecording();
            else
                StartCaptureOperation(x, y, CaptureOperation.PlayPiano);
        }

        protected override void OnTouchFling(int x, int y, float velX, float velY)
        {
            if (IsPointInPanRectangle(lastX, lastY) && canFling)
            {
                EndCaptureOperation(x, y);
                flingVelX = velX;
            }
        }

        protected override void OnTouchScaleBegin(int x, int y)
        {
            lastX = x;
            lastY = y;

            if (captureOperation != CaptureOperation.None)
            {
                Debug.Assert(captureOperation != CaptureOperation.MobileZoom);
                EndCaptureOperation(x, y);
            }

            StartCaptureOperation(x, y, CaptureOperation.MobileZoom);
        }

        protected override void OnTouchScale(int x, int y, float scale)
        {
            UpdateCaptureOperation(x, y, scale);
            lastX = x;
            lastY = y;
        }

        protected override void OnTouchScaleEnd(int x, int y)
        {
            EndCaptureOperation(x, y);
            lastX = x;
            lastY = y;
        }

        protected override void OnTouchMove(int x, int y)
        {
            UpdateCaptureOperation(x, y);
            lastX = x;
            lastY = y;
        }
    }
}