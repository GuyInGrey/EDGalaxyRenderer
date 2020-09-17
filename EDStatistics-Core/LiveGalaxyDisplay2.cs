using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.ES11;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;

namespace EDStatistics_Core
{
    public class LiveGalaxyDisplay2 : GameWindow
    {
        int VertexBufferObject;

        public LiveGalaxyDisplay2() : base(new GameWindowSettings()
        {
            RenderFrequency = 60,
            UpdateFrequency = 60,
        }, new NativeWindowSettings()
        {
            Size = new OpenTK.Mathematics.Vector2i(1000, 1000),
            Title = "Elite: Dangerous Exploration and Galaxy Renderer",
            StartFocused = true,
        })
        {
            
        }

        protected override void OnLoad()
        {
            float[] vertices = {
                -0.5f, -0.5f, 0.0f, //Bottom-left vertex
                 0.5f, -0.5f, 0.0f, //Bottom-right vertex
                 0.0f,  0.5f, 0.0f  //Top vertex
            };

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTargetArb.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTargetArb.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageArb.StaticDraw);

            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.ClearColor(.39f, .58f, .92f, 1f);




            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (KeyboardState.IsKeyDown(Key.Escape)) { Close(); }
            base.OnUpdateFrame(args);
        }


        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTargetArb.ArrayBuffer, 0);
            GL.DeleteBuffer(VertexBufferObject);
            base.OnUnload();
        }
    }
}
