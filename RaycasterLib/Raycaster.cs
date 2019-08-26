using System;
using System.Collections.Generic;

namespace LunarLabs.Raycaster
{
    public enum HitAxis
    {
        X,
        Y
    }

    public struct MapTile
    {
        public int height;
        public byte wallID;
        public byte ceilID;
        public byte floorID;
        public float lightLevel;
        public bool hasLight;
    }

    public class Camera
    {
        private readonly Raycaster raycaster;

        public float posX { get; private set; } = 22.0f;
        public float posY { get; private set; } = 11.5f; //x and y start position

        public float dirX { get; private set; } = -1.0f;
        public float dirY { get; private set; } = 0.0f; //initial direction vector

        public float planeX { get; private set; } = 0.0f;
        public float planeY { get; private set; } = 0.66f; //the 2d raycaster version of camera plane

        public Camera(Raycaster raycaster)
        {
            this.raycaster = raycaster;
        }

        public void Move(float moveSpeed)
        {
            posX += dirX * moveSpeed;
            posY += dirY * moveSpeed;
        }

        public void Strafe(float moveSpeed)
        {
            posX += dirY * moveSpeed;
            posY -= dirX * moveSpeed;
        }

        public int drawOffset { get; private set; }
        internal int minY;
        internal int maxY;

        private float lookOffset = 0;


        public void Look(float speed)
        {
            speed *= 128;
            lookOffset += speed;

            Update();
        }

        internal void Update()
        {
            int limit = 64;
            if (lookOffset < -limit) lookOffset = -limit;
            if (lookOffset > limit) lookOffset = limit;

            drawOffset = MathUtils.FloorToInt(lookOffset);
            minY = -drawOffset;
            maxY = (raycaster.Output.Height - 1) - drawOffset;
        }

        public void Rotate(float rotSpeed)
        {
            float oldDirX = dirX;
            dirX = dirX * MathUtils.Cos(rotSpeed) - dirY * MathUtils.Sin(rotSpeed);
            dirY = oldDirX * MathUtils.Sin(rotSpeed) + dirY * MathUtils.Cos(rotSpeed);
            float oldPlaneX = planeX;
            planeX = planeX * MathUtils.Cos(rotSpeed) - planeY * MathUtils.Sin(rotSpeed);
            planeY = oldPlaneX * MathUtils.Sin(rotSpeed) + planeY * MathUtils.Cos(rotSpeed);
        }
    }

    public abstract class Raycaster
    {
        public readonly Texture Output;
        public readonly Camera Camera;

        public int tileSize = 64;

        private bool initialized = false;

        //1D Zbuffer
        internal float[] ZBuffer;

        public struct SpriteQueue
        {
            public Sprite sprite;
            public float distance;
        }

        //arrays used to sort the sprites
        private SpriteQueue[] spriteQueue;
        private List<Sprite> sprites = new List<Sprite>();

        internal Texture[] textures;
        private Texture[] skybox = new Texture[6];

        public Raycaster(int resolutionX, int resolutionY)
        {
            Output = new Texture(resolutionX, resolutionY);
            ZBuffer = new float[resolutionX];
            spriteQueue = new SpriteQueue[999];
            textures = new Texture[256];

            Camera = new Camera(this);

            initialized = false;
        }

        public void AddSprite(Sprite sprite)
        {
            sprites.Add(sprite);
        }

        private void Init()
        { 
            Camera.Update();

            for (int k = 1; k < textures.Length; k++)
                textures[k] = LoadTile(k);

            for (int i=0; i<skybox.Length; i++)
            {
                skybox[i] = LoadSkybox(i);
            }
        }

        protected abstract Texture LoadTile(int index);
        protected abstract Texture LoadSkybox(int side);
        protected abstract bool GetTileAt(int x, int y, out MapTile tile);

        internal float CalculateFog(float dist, int floorX, int floorY, float u, float v, bool emissive)
        {
            float limit = 3;
            float scale;
            if (dist < limit)
            {
                scale = 1;
            }
            else
            {
                dist -= limit;
                scale = 1 - MathUtils.Min(1, MathUtils.Abs(dist / 8.0f));
            }

            return scale;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        //function used to sort the sprites
        private void CombSort()
        {
            int amount = sprites.Count;
            int gap = amount;
            bool swapped = false;
            while (gap > 1 || swapped)
            {
                //shrink factor 1.3
                gap = (gap * 10) / 13;
                if (gap == 9 || gap == 10) gap = 11;
                if (gap < 1) gap = 1;
                swapped = false;
                for (int i = 0; i < amount - gap; i++)
                {
                    int j = i + gap;
                    if (spriteQueue[i].distance < spriteQueue[j].distance)
                    {
                        Swap<SpriteQueue>(ref spriteQueue[i], ref spriteQueue[j]);
                        swapped = true;
                    }
                }
            }
        }

        public void Render()
        {
            if (!initialized)
            {
                initialized = true;
                Init();
            }

            int screenWidth = Output.Width;
            int screenHeight = Output.Height;

            #region ENVIROMENT CASTING
            for (int x = 0; x < screenWidth; x++)
            {
                //calculate ray position and direction
                float cameraX = 2 * x / (float)(screenWidth) - 1; //x-coordinate in camera space
                float rayPosX = Camera.posX;
                float rayPosY = Camera.posY;
                float rayDirX = Camera.dirX + Camera.planeX * cameraX;
                float rayDirY = Camera.dirY + Camera.planeY * cameraX;

                //which box of the map we're in
                int mapX = (int)(rayPosX);
                int mapY = (int)(rayPosY);

                //length of ray from current position to next x or y-side
                float sideDistX;
                float sideDistY;

                //length of ray from one x or y-side to next x or y-side
                float deltaDistX = MathUtils.Sqrt(1 + (rayDirY * rayDirY) / (rayDirX * rayDirX));
                float deltaDistY = MathUtils.Sqrt(1 + (rayDirX * rayDirX) / (rayDirY * rayDirY));
                float perpWallDist;

                //what direction to step in x or y-direction (either +1 or -1)
                int stepX;
                int stepY;

                HitAxis side = HitAxis.X; //was a NS or a EW wall hit?

                //calculate step and initial sideDist
                if (rayDirX < 0)
                {
                    stepX = -1;
                    sideDistX = (rayPosX - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0f - rayPosX) * deltaDistX;
                }
                if (rayDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (rayPosY - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1.0f - rayPosY) * deltaDistY;
                }

                Texture wallTex = null;

                //perform DDA
                while (true)
                {
                    //jump to next map square, OR in x-direction, OR in y-direction
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = HitAxis.X;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = HitAxis.Y;
                    }

                    //Check if ray has hit a wall
                    MapTile tile;
                    var hit = GetTileAt(mapX, mapY, out tile);
                    wallTex = textures[tile.wallID];

                    if (!hit || tile.wallID > 0)
                    {
                        break;
                    }
                }

                //Calculate distance of perpendicular ray (oblique distance will give fisheye effect!)
                if (side == 0)
                    perpWallDist = (mapX - rayPosX + (1 - stepX) / 2) / rayDirX;
                else
                    perpWallDist = (mapY - rayPosY + (1 - stepY) / 2) / rayDirY;

                //Calculate height of line to draw on screen
                int lineHeight = MathUtils.FloorToInt(screenHeight / perpWallDist);

                //calculate lowest and highest pixel to fill in current stripe
                int drawStart = (-lineHeight + screenHeight) / 2;
                if (drawStart < Camera.minY) drawStart = Camera.minY;

                int drawEnd = (lineHeight + screenHeight) / 2;
                if (drawEnd > Camera.maxY) drawEnd = Camera.maxY;

                int tempEnd = drawEnd;
                //drawStart += (int) (180  / perpWallDist);
                //drawEnd -= (int)(60 / perpWallDist);

                //calculate value of wallX
                float wallX, wallY; //where exactly the wall was hit

                wallX = rayPosY + perpWallDist * rayDirY;
                wallY = rayPosX + perpWallDist * rayDirX;

                float dist = perpWallDist;

                wallX = (side == HitAxis.Y) ? wallY : wallX;

                wallX -= MathUtils.Floor(wallX);

                //x coordinate on the texture
                int texX;

                texX = MathUtils.FloorToInt(wallX * tileSize);

                if (side == HitAxis.X && rayDirX > 0) texX = tileSize - texX - 1;
                if (side == HitAxis.Y && rayDirY < 0) texX = tileSize - texX - 1;

                for (int y = drawStart; y < drawEnd; y++)
                {
                    float d = y - screenHeight * 0.5f + lineHeight * 0.5f;
                    int texY = MathUtils.FloorToInt(Math.Abs(((d * tileSize) / lineHeight)));

                    byte red, green, blue, alpha;
                    float scale;

                    if (wallTex != null)
                    {
                        wallTex.GetPixel(texX, texY, out red, out green, out blue, out alpha);
                        scale = CalculateFog(dist, mapX, mapY, texX, texY, false);

                        if (side == HitAxis.Y)
                        {
                            scale *= 0.5f;
                        }
                    }
                    else
                    {
                        SampleSky(rayDirX, rayDirY, y, out red, out green, out blue, out alpha);
                        scale = 1.0f;
                    }

                    //make color darker for y-sides: R, G and B byte each divided through two with a "shift" and an "and"

                    
                    WritePixel(x, y, texX, texY, red, green, blue, scale);
                }
                drawEnd = tempEnd;

                //SET THE ZBUFFER FOR THE SPRITE CASTING
                ZBuffer[x] = perpWallDist; //perpendicular distance is used

                //FLOOR CASTING
                float floorXWall, floorYWall; //x, y position of the floor texel at the bottom of the wall

                //4 different wall directions possible
                switch (side)
                {
                    case HitAxis.X:
                        {
                            if (rayDirX > 0)
                            {
                                floorXWall = mapX;
                            }
                            else
                            {
                                floorXWall = mapX + 1.0f;
                            }
                            floorYWall = mapY + wallX;
                            break;
                        }

                    default: // Y
                        {
                            if (rayDirY > 0)
                            {
                                floorXWall = mapX + wallX;
                                floorYWall = mapY;
                            }
                            else
                            {
                                floorXWall = mapX + wallX;
                                floorYWall = mapY + 1.0f;
                            }
                            break;
                        }
                }


                float distWall, distPlayer, currentDist;

                distWall = perpWallDist;
                distPlayer = 0.0f;

                if (wallTex != null)
                {
                    //drawEnd++;
                }

                //draw the floor from drawEnd to the bottom of the screen
                for (int y = drawEnd; y < Camera.maxY; y++)
                {
                    var temp = (2.0f * y) - screenHeight;
                    currentDist = screenHeight / temp; //you could make a small lookup table for this instead

                    float weight = (currentDist - distPlayer) / (distWall - distPlayer);

                    float currentFloorX = MathUtils.Lerp(Camera.posX, floorXWall, weight);
                    float currentFloorY = MathUtils.Lerp(Camera.posY, floorYWall, weight);

                    mapX = MathUtils.FloorToInt(currentFloorX);
                    mapY = MathUtils.FloorToInt(currentFloorY);

                    MapTile tile;
                    GetTileAt(mapX, mapY, out tile);

                    dist = (currentDist - distPlayer);

                    byte red, green, blue, alpha;
                    float scale;

                    int floorTexX, floorTexY;

                    if (tile.floorID > 0)
                    {
                        var floorTexture = textures[tile.floorID];
                        floorTexX = (int)(currentFloorX * tileSize) % floorTexture.Width;
                        floorTexY = (int)(currentFloorY * tileSize) % floorTexture.Height;

                        scale = CalculateFog(dist, mapX, mapY, floorTexX, floorTexY, false);
                        floorTexture.GetPixel(floorTexX, floorTexY, out red, out green, out blue, out alpha);
                    }
                    else
                    {
                        scale = 1.0f;
                        floorTexX = 0;
                        floorTexY = 0;
                        SampleSky(rayDirX, rayDirY, y, out red, out green, out blue, out alpha);
                    }

                    WritePixel(x, y, floorTexX, floorTexY, red, green, blue, scale);
                }

                //ceiling
                for (int y = Camera.minY; y < drawStart; y++)
                {
                    var temp = (2.0f * (screenHeight- y)) - screenHeight;
                    currentDist = screenHeight / temp; //you could make a small lookup table for this instead

                    float weight = (currentDist - distPlayer) / (distWall - distPlayer);

                    float currentFloorX = MathUtils.Lerp(Camera.posX, floorXWall, weight);
                    float currentFloorY = MathUtils.Lerp(Camera.posY, floorYWall, weight);

                    mapX = MathUtils.FloorToInt(currentFloorX);
                    mapY = MathUtils.FloorToInt(currentFloorY);

                    MapTile tile;
                    GetTileAt(mapX, mapY, out tile);

                    int ceilTexX, ceilTexY;
                    float scale;

                    dist = (currentDist - distPlayer);
                    byte red, green, blue, alpha;

                    if (tile.ceilID > 0)
                    {
                        var ceilTexture = textures[tile.ceilID];
                        ceilTexX = (int)(currentFloorX * tileSize) % ceilTexture.Width;
                        ceilTexY = (int)(currentFloorY * tileSize) % ceilTexture.Height;
                        scale = CalculateFog(dist, mapX, mapY, ceilTexX, ceilTexY, false);

                        ceilTexture.GetPixel(ceilTexX, ceilTexY, out red, out green, out blue, out alpha);
                    }
                    else
                    {
                        ceilTexX = 0;
                        ceilTexY = 0;
                        scale = 1;
                        SampleSky(rayDirX, rayDirY, y, out red, out green, out blue, out alpha);
                    }

                    WritePixel(x, y, ceilTexX, ceilTexY, red, green, blue, scale);
                }
            }
            #endregion

            #region SPRITE CASTING
            /*bool hasDead = false;
            for (int i = 0; i < sprites.Count; i++)
            {
                var s = sprites[i];
                s.Move(this);
                if (s.shouldRemove)
                {
                    hasDead = true;
                }
            }

            if (hasDead)
            {
                sprites.RemoveAll(s => s.shouldRemove);
            }*/

            //sort sprites from far to close
            for (int i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];
                spriteQueue[i].sprite = sprite;
                spriteQueue[i].distance = ((Camera.posX - sprite.x) * (Camera.posX - sprite.x) + (Camera.posY - sprite.y) * (Camera.posY - sprite.y)); //sqrt not taken, unneeded
            }
            CombSort();

            float invDet = 1.0f / (Camera.planeX * Camera.dirY - Camera.dirX * Camera.planeY); //required for correct matrix multiplication

            //after sorting the sprites, do the projection and draw them
            for (int i = 0; i < sprites.Count; i++)
            {
                var sprite = spriteQueue[i].sprite;
                sprite.Render(this, invDet);
            }
            #endregion
        }

        internal void WritePixel(int x, int y, int u, int v, byte red, byte green, byte blue, float scale)
        {
            y += Camera.drawOffset;

            // todo : support scale + dither

            red = (byte)(red * scale);
            green = (byte)(green * scale);
            blue = (byte)(blue * scale);

            Output.SetPixel(x, y, red, green, blue);
        }

        private void ConvertCoordsToCubemap(float x, float y, float z, out int index, out float u, out float v)
        {
            float absX = MathUtils.Abs(x);
            float absY = MathUtils.Abs(y);
            float absZ = MathUtils.Abs(z);

            bool isXPositive = x > 0;
            bool isYPositive = y > 0;
            bool isZPositive = z > 0;

            float maxAxis, uc, vc;

            // POSITIVE X
            if (isXPositive && absX >= absY && absX >= absZ)
            {
                // u (0 to 1) goes from +z to -z
                // v (0 to 1) goes from -y to +y
                maxAxis = absX;
                uc = -z;
                vc = y;
                index = 0;
            }
            else
            // NEGATIVE X
            if (!isXPositive && absX >= absY && absX >= absZ)
            {
                // u (0 to 1) goes from -z to +z
                // v (0 to 1) goes from -y to +y
                maxAxis = absX;
                uc = z;
                vc = y;
                index = 1;
            }
            else
            // POSITIVE Y
            if (isYPositive && absY >= absX && absY >= absZ)
            {
                // u (0 to 1) goes from -x to +x
                // v (0 to 1) goes from +z to -z
                maxAxis = absY;
                uc = x;
                vc = -z;
                index = 2;
            }
            else
            // NEGATIVE Y
            if (!isYPositive && absY >= absX && absY >= absZ)
            {
                // u (0 to 1) goes from -x to +x
                // v (0 to 1) goes from -z to +z
                maxAxis = absY;
                uc = x;
                vc = z;
                index = 3;
            }
            else
            // POSITIVE Z
            if (isZPositive && absZ >= absX && absZ >= absY)
            {
                // u (0 to 1) goes from -x to +x
                // v (0 to 1) goes from -y to +y
                maxAxis = absZ;
                uc = x;
                vc = y;
                index = 4;
            }
            else
            // NEGATIVE Z
            //if (!isZPositive && absZ >= absX && absZ >= absY)
            {
                // u (0 to 1) goes from +x to -x
                // v (0 to 1) goes from -y to +y
                maxAxis = absZ;
                uc = -x;
                vc = y;
                index = 5;
            }

            // Convert range from -1 to 1 to 0 to 1
            u = 0.5f * (uc / maxAxis + 1.0f);
            v = 0.5f * (vc / maxAxis + 1.0f);

            u = 1 - u;
        }

        private void SampleSky(float rayDirX, float rayDirY, int y, out byte red, out byte green, out byte blue, out byte alpha)
        {
            int index;
            float u, v;

            y += 64;
            //y += drawOffset / 2;

            var n = new Vector3(rayDirX, 0, rayDirY);
            n.Normalize();
            ConvertCoordsToCubemap(n.X, n.Y, n.Z, out index, out u, out v);

            v = (y / (float)Output.Height);

            var texture = skybox[index];

            int texX = (int)(u * texture.Width);
            int texY = (int)(v * texture.Height);

            texture.GetPixel(texX, texY, out red, out green, out blue, out alpha);
        }
    }
}
