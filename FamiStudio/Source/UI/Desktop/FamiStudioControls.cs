﻿using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace FamiStudio
{
    public class FamiStudioControls
    {
        private int width;
        private int height;
        private GLGraphics gfx;
        private GLControl[] controls = new GLControl[4];

        private Toolbar toolbar;
        private Sequencer sequencer;
        private PianoRoll pianoRoll;
        private ProjectExplorer projectExplorer;

        public Toolbar ToolBar => toolbar;
        public Sequencer Sequencer => sequencer;
        public PianoRoll PianoRoll => pianoRoll;
        public ProjectExplorer ProjectExplorer => projectExplorer;
        public GLControl[] Controls => controls;

        public FamiStudioControls(FamiStudioForm parent)
        {
            toolbar = new Toolbar();
            sequencer = new Sequencer();
            pianoRoll = new PianoRoll();
            projectExplorer = new ProjectExplorer();

            controls[0] = toolbar;
            controls[1] = sequencer;
            controls[2] = pianoRoll;
            controls[3] = projectExplorer;

            foreach (var ctrl in controls)
                ctrl.ParentForm = parent;
        }

        public void Resize(int w, int h)
        {
            width  = w;
            height = h;

            int toolBarHeight = (int)(40 * GLTheme.MainWindowScaling);
            int projectExplorerWidth = (int)(280 * GLTheme.MainWindowScaling);
            int sequencerHeight = pianoRoll.IsMaximized ? 1 : (int)(sequencer.ComputeDesiredSizeY() * GLTheme.MainWindowScaling);

            toolbar.Move(0, 0, width, toolBarHeight);
            projectExplorer.Move(width - projectExplorerWidth, toolBarHeight, projectExplorerWidth, height - toolBarHeight);
            sequencer.Move(0, toolBarHeight, width - projectExplorerWidth, sequencerHeight);
            pianoRoll.Move(0, toolBarHeight + sequencerHeight, width - projectExplorerWidth, height - toolBarHeight - sequencerHeight);
        }

        public GLControl GetControlAtCoord(int formX, int formY, out int ctrlX, out int ctrlY)
        {
            foreach (var ctrl in controls)
            {
                ctrlX = formX - ctrl.Left;
                ctrlY = formY - ctrl.Top;

                if (ctrlX >= 0 &&
                    ctrlY >= 0 &&
                    ctrlX <  ctrl.Width &&
                    ctrlY <  ctrl.Height)
                {
                    return ctrl;
                }
            }

            ctrlX = 0;
            ctrlY = 0;
            return null;
        }

        public void Invalidate()
        {
            foreach (var ctrl in controls)
                ctrl.Invalidate();
        }

        public unsafe bool Redraw()
        {
            bool anyNeedsRedraw = false;
            foreach (var control in controls)
                anyNeedsRedraw |= control.NeedsRedraw;

            if (anyNeedsRedraw)
            {
                // Tentative fix for a bug when NSF dialog is open that I can no longer repro.
                if (controls[0].App.Project == null)
                    return true;

                gfx.BeginDrawFrame();

                foreach (var control in controls)
                {
                    gfx.BeginDrawControl(new System.Drawing.Rectangle(control.Left, control.Top, control.Width, control.Height), height);
                    control.Render(gfx);
                    control.Validate();
                }

                gfx.EndDrawFrame();

                return true;
            }

            return false;
        }

        public void InitializeGL()
        {
            gfx = new GLGraphics();
            foreach (var ctrl in controls)
                ctrl.RenderInitialized(gfx);
        }
    }
}