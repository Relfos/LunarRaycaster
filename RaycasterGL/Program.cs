using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK.Input;
using LunarLabs.Raycaster;

namespace LunarLabs.Framework
{
    public class CustomRaycaster : Raycaster.Raycaster
    {
        private Bitmap tileset;
        private Bitmap skybox;

        private const int mapWidth = 24;
        private const int mapHeight = 24;

        private byte[,] wallData = new byte[,]
        {
              {4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,7,7,7,7,7,7,7,7},
              {4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,7,0,0,0,0,0,0,7},
              {4,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,7},
              {4,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,7},
              {4,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,7,0,0,0,0,0,0,7},
              {4,0,4,0,0,0,0,5,5,5,5,5,5,5,5,5,7,7,0,7,7,7,7,7},
              {4,0,5,0,0,0,0,5,0,5,0,5,0,5,0,5,7,0,0,0,7,7,7,1},
              {4,0,6,0,0,0,0,5,0,0,0,0,0,0,0,5,7,0,0,0,0,0,0,8},
              {4,0,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,7,7,7,1},
              {4,0,8,0,0,0,0,5,0,0,0,0,0,0,0,5,7,0,0,0,0,0,0,8},
              {4,0,0,0,0,0,0,5,0,0,0,0,0,0,0,5,7,0,0,0,7,7,7,1},
              {4,0,0,0,0,0,0,5,5,5,5,0,5,5,5,5,7,7,7,7,7,7,7,1},
              {6,6,6,6,6,6,6,6,6,6,6,9,6,6,6,6,6,6,6,6,6,6,6,6},
              {8,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4},
              {6,6,6,6,6,6,0,6,6,6,6,0,6,6,6,6,6,6,6,6,6,6,6,6},
              {4,4,4,4,4,4,0,4,4,4,6,0,6,2,2,2,2,2,2,2,3,3,3,3},
              {4,0,0,0,0,0,0,0,0,4,6,0,6,2,0,0,0,0,0,2,0,0,0,2},
              {4,0,0,0,0,0,0,0,0,0,0,0,6,2,0,0,5,0,0,2,0,0,0,2},
              {4,0,0,0,0,0,0,0,0,4,6,0,6,2,0,0,0,0,0,2,2,0,2,2},
              {4,0,6,0,6,0,0,0,0,4,6,0,0,0,0,0,5,0,0,0,0,0,0,2},
              {4,0,0,5,0,0,0,0,0,4,6,0,6,2,0,0,0,0,0,2,2,0,2,2},
              {4,0,6,0,6,0,0,0,0,4,6,0,6,2,0,0,5,0,0,2,0,0,0,2},
              {4,0,0,0,0,0,0,0,0,4,6,0,6,2,0,0,0,0,0,2,0,0,0,2},
              {4,4,4,4,4,4,4,4,4,4,1,1,1,2,2,2,2,2,2,3,3,3,3,3}
        };

        public CustomRaycaster():base(160, 144)
        {
            tileset = new Bitmap(Image.FromFile("Assets/tileset.png"));
            skybox = new Bitmap(Image.FromFile("Assets/skybox.png"));

            AddSprite(new Sprite(22, 11.5f, 11));
            AddSprite(new Sprite(20, 11.5f, 10, 4));
            AddSprite(new Sprite(18.5f, 11.5f, 12));
            AddSprite(new Sprite(17, 11.5f, 10));
            AddSprite(new Sprite(10, 11.5f, 11));

            AddSprite(new Sprite(15, 11.5f, 13, 0, 2, 0));
        }

        public override bool GetTileAt(int x, int y, out MapTile tile)
        {
            tile = new MapTile();
            tile.lightID = 0;
            tile.lightLevel = 1.0f;
            tile.wallHeight = 0;
            tile.lightLevel = 1.0f;
            tile.ceilHeight = 0;

            if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight)
            {
                tile.wallID = 0;
                tile.floorID = 0;
                tile.ceilID = 0;

                int limit = 2;
                if (x<-limit || y<-limit || x >= mapWidth + limit || y >= mapHeight + limit)
                {
                    return false;
                }

                return true;
            }
            else
            {
                tile.wallID = wallData[x, y];
                tile.floorID = 4;
                tile.ceilID = (byte)(y >=9 && y<=13?7: 0);
                tile.ceilHeight = (byte)( y == 9? 24:0);

                if (x == 18 && y == 11)
                {
                    tile.lightID = 15;
                }

                if (x == 17 && y == 9)
                {
                    tile.ceilHeight = 20;
                }

                if (x == 16 && y == 10)
                {
                    tile.wallHeight = 24;
                    tile.wallID = 3;
                    tile.floorID = 7;
                }
                if (x == 15 && y == 10)
                {
                    tile.wallHeight = 16;
                    tile.wallID = 3;
                    tile.floorID = 7;
                }

                if (tile.ceilID != 0)
                {
                    tile.lightLevel = 0.5f;
                }

                return true;
            }
        }

        protected override Texture LoadSkybox(int side)
        {
            int scale = 2;
            int size = 64 * scale;

            switch (side)
            {
                case 0: return Texture.Crop(0 * size, 1 * size, size, size, (x, y) => skybox.GetPixel(x, y)); // left
                case 1: return Texture.Crop(2 * size, 1 * size, size, size, (x, y) => skybox.GetPixel(x, y)); // right
                case 2: return Texture.Crop(1 * size, 0 * size, size, size, (x, y) => skybox.GetPixel(x, y)); // up
                case 3: return Texture.Crop(1 * size, 2 * size, size, size, (x, y) => skybox.GetPixel(x, y)); // down
                case 4: return Texture.Crop(1 * size, 1 * size, size, size, (x, y) => skybox.GetPixel(x, y)); // front
                case 5: return Texture.Crop(3 * size, 1 * size, size, size, (x, y) => skybox.GetPixel(x, y)); // back
                default: return null;
            }
        }

        protected override Texture LoadTile(int index)
        {
            int maxTile = (tileset.Width / TileSize);
            if (index == 0 || index > maxTile)
            {
                return null;
            }

            return Texture.Crop((index - 1) * TileSize, 0, TileSize, TileSize, (x, y) => tileset.GetPixel(x, y));
        }
    }

    class Program
    {
        static Raycaster.Raycaster engine;

        static void UpdateBuffer(int texID)
        {
            GL.BindTexture(TextureTarget.Texture2D, texID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, engine.Output.Width, engine.Output.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, engine.Output.Pixels);
        }

        public static void DrawBuffer(OpenTK.GameWindow game, int textureID)
        {
            float u1 = 0;
            float u2 = 1;
            float v1 = 0;
            float v2 = 1;

            float w = 1;
            float h = 1;


            float px = 0;
            float py = 0;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0.0, 1.0, 0.0, 1.0, 0.0, 4.0);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            GL.Begin(PrimitiveType.Quads);

            GL.Color3(Color.White);
            GL.TexCoord2(u1, v1);
            GL.Vertex2(px + 0, py + h);

            GL.TexCoord2(u1, v2);
            GL.Vertex2(px + 0, py + 0);

            GL.TexCoord2(u2, v2);
            GL.Vertex2(px + w, py + 0);

            GL.TexCoord2(u2, v1);
            GL.Vertex2(px + w, py + h);

            GL.End();
        }

        static int scale = 4;

        static HashSet<Key> input = new HashSet<Key>();

        static void Main(string[] args)
        {
            var timer = new Stopwatch();
            timer.Start();

            int bufferTexID = 0;

            int lastMouseX = -1;
            int lastMouseY = -1;

            float mouseDeltaX = 0;
            float mouseDeltaY = 0;
            float newMouseDeltaX = 0;
            float newMouseDeltaY = 0;

            engine = new CustomRaycaster();

            using (var game = new OpenTK.GameWindow(engine.Output.Width * scale, engine.Output.Height * scale, GraphicsMode.Default, "Raycasting"))
            {
                game.Load += (sender, e) =>
                {
                    // setup settings, load textures, sounds
                    game.VSync = OpenTK.VSyncMode.On;

                    bufferTexID = GL.GenTexture();
                    UpdateBuffer(bufferTexID);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                    game.Cursor = OpenTK.MouseCursor.Empty;
                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.KeyDown += (sender, e) =>
                {
                    input.Add(e.Key);
                };

                game.KeyUp += (sender, e) =>
                {
                    input.Remove(e.Key);
                };

                game.MouseMove += (sender, e) =>
                {
                    if (lastMouseX != -1)
                    {
                        newMouseDeltaX = (lastMouseX - e.X) / (float)game.Width;
                        newMouseDeltaY = (lastMouseY - e.Y) / (float)game.Height;
                    }

                    lastMouseX = e.X;
                    lastMouseY = e.Y;
                };

                game.UpdateFrame += (sender, e) =>
                {
                    // add game logic, input handling
                    if (input.Contains(Key.Escape))
                    {
                        game.Exit();
                        Environment.Exit(0);
                    }

                    float speed = 0.02f;

                    if (input.Contains(Key.W))
                    {
                        engine.Camera.Move(speed);
                    }
                    if (input.Contains(Key.S))
                    {
                        engine.Camera.Move(-speed);
                    }
                    if (input.Contains(Key.A))
                    {
                        engine.Camera.Strafe(-speed);
                    }
                    if (input.Contains(Key.D))
                    {
                        engine.Camera.Strafe(speed);
                    }

                    if (input.Contains(Key.Left))
                    {
                        engine.Camera.Rotate(speed);
                    }
                    if (input.Contains(Key.Right))
                    {
                        engine.Camera.Rotate(-speed);
                    }

                    if (input.Contains(Key.Up))
                    {
                        engine.Camera.Look(speed);
                    }
                    if (input.Contains(Key.Down))
                    {
                        engine.Camera.Look(-speed);
                    }

                    mouseDeltaX = Mathf.Lerp(mouseDeltaX, newMouseDeltaX, 0.5f);
                    mouseDeltaY = Mathf.Lerp(mouseDeltaY, newMouseDeltaY, 0.5f);

                    engine.Camera.Rotate(mouseDeltaX * 32);
                    engine.Camera.Look(mouseDeltaY * 32);

                    newMouseDeltaX = 0;
                    newMouseDeltaY = 0;
                    if (game.Focused)
                    { 
                        OpenTK.Input.Mouse.SetPosition(game.X + game.Width / 2, game.Y + game.Height / 2);
                    }
                };

                game.Closed += (sender, e) =>
                {
                    //shouldStop = true;
                };

                long lastTick = timer.ElapsedMilliseconds;

                game.RenderFrame += (sender, e) =>
                {
                    engine.Render();

                    UpdateBuffer(bufferTexID);
                    DrawBuffer(game, bufferTexID);

                    game.SwapBuffers();

                    lastTick = timer.ElapsedMilliseconds;
                };

                game.Run();
            }
        }
    }
}
