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
        public int cutOff;
        public float lightLevel;
        public bool hasLight;
    }

    public abstract class Raycaster
    {
        public const int TileShift = 5;
        public const int TileSize = 1 << TileShift;
        public const int HalfTileSize = TileSize >> 1;

        public readonly Texture Output;
        public readonly Camera Camera;

        private bool initialized = false;

        //1D Zbuffer
        internal float[] ZBuffer;
        internal float[] depthBuffer;

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

        public struct MapHit
        {
            public int mapX;
            public int mapY;
            public Texture wallTex;
            public int cutOff;
            public HitAxis side;

            public MapHit(int mapX, int mapY, HitAxis side, Texture wall, int cutOff)
            {
                this.mapX = mapX;
                this.mapY = mapY;
                this.side = side;
                this.cutOff = cutOff;
                this.wallTex = wall;
            }
        }

        private MapHit[] hits = new MapHit[8];

        public Raycaster(int resolutionX, int resolutionY)
        {
            Output = new Texture(resolutionX, resolutionY);
            ZBuffer = new float[resolutionX];
            depthBuffer = new float[resolutionX*resolutionY];
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

            Array.Clear(Output.Pixels, 0, Output.Pixels.Length);

            #region ENVIROMENT CASTING
            for (int x = 0; x < screenWidth; x++)
            {
                for (int y=0; y<screenHeight; y++)
                {
                    depthBuffer[x + y * screenWidth] = 9999;
                }

                //calculate ray position and direction
                float cameraX = 2 * x / (float)(screenWidth) - 1; //x-coordinate in camera space
                float rayPosX = Camera.posX;
                float rayPosY = Camera.posY;
                float rayDirX = Camera.dirX + Camera.planeX * cameraX;
                float rayDirY = Camera.dirY + Camera.planeY * cameraX;

                //length of ray from current position to next x or y-side
                float sideDistX;
                float sideDistY;

                //length of ray from one x or y-side to next x or y-side
                float deltaDistX = MathUtils.Sqrt(1 + (rayDirY * rayDirY) / (rayDirX * rayDirX));
                float deltaDistY = MathUtils.Sqrt(1 + (rayDirX * rayDirX) / (rayDirY * rayDirY));

                //what direction to step in x or y-direction (either +1 or -1)
                int stepX;
                int stepY;

                //which box of the map we're in
                int currentMapX = (int)(rayPosX);
                int currentMapY = (int)(rayPosY);

                //calculate step and initial sideDist
                if (rayDirX < 0)
                {
                    stepX = -1;
                    sideDistX = (rayPosX - currentMapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (currentMapX + 1.0f - rayPosX) * deltaDistX;
                }
                if (rayDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (rayPosY - currentMapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (currentMapY + 1.0f - rayPosY) * deltaDistY;
                }

                int hitCount = 0;
                HitAxis currentSide; //was a NS or a EW wall hit?

                //perform DDA
                while (true)
                {
                    //jump to next map square, OR in x-direction, OR in y-direction
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        currentMapX += stepX;
                        currentSide = HitAxis.X;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        currentMapY += stepY;
                        currentSide = HitAxis.Y;
                    }

                    //Check if ray has hit a wall
                    MapTile tile;
                    var sucess = GetTileAt(currentMapX, currentMapY, out tile);
                    var wallTex = textures[tile.wallID];

                    if (!sucess)
                    {
                        var hit = new MapHit(currentMapX, currentMapY, currentSide, wallTex, 0);
                        hits[hitCount] = hit;
                        hitCount++;
                        break;
                    }

                    if (tile.wallID > 0)
                    {
                        var hit = new MapHit(currentMapX, currentMapY, currentSide, wallTex, tile.cutOff);
                        hits[hitCount] = hit;
                        hitCount++;

                        if (!wallTex.hasAlpha && tile.cutOff == 0)
                        {
                            break;
                        }
                    }

                    if (hitCount >= hits.Length)
                    {
                        break;
                    }
                }

                float perpWallDist = 999;

                int drawStart = Camera.maxY;
                int drawEnd = Camera.minY;

                //where exactly the wall was hit
                float wallX = 0;
                float wallY = 0;
                
                if (hitCount > 1)
                {
                    hitCount+=0;
                }

                for (int k= 0; k< hitCount; k++)
                {
                    var hit = hits[k];

                    //Calculate distance of perpendicular ray (oblique distance will give fisheye effect!)
                    if (hit.side == 0)
                        perpWallDist = (hit.mapX - rayPosX + (1 - stepX) / 2) / rayDirX;
                    else
                        perpWallDist = (hit.mapY - rayPosY + (1 - stepY) / 2) / rayDirY;

                    //Calculate height of line to draw on screen
                    int lineHeight = MathUtils.FloorToInt(screenHeight / perpWallDist);

                    //calculate lowest and highest pixel to fill in current stripe
                    drawStart = (-lineHeight + screenHeight) / 2;
                    if (drawStart < Camera.minY) drawStart = Camera.minY;

                    drawEnd = (lineHeight + screenHeight) / 2;
                    if (drawEnd > Camera.maxY) drawEnd = Camera.maxY;

                    int tempEnd = drawEnd;
                    //drawStart += (int) (180  / perpWallDist);
                    //drawEnd -= (int)(60 / perpWallDist);

                    //calculate value of wallX
                    wallX = rayPosY + perpWallDist * rayDirY;
                    wallY = rayPosX + perpWallDist * rayDirX;

                    float dist = perpWallDist;

                    wallX = (hit.side == HitAxis.Y) ? wallY : wallX;

                    wallX -= MathUtils.Floor(wallX);

                    //x coordinate on the texture
                    int texX;

                    texX = MathUtils.FloorToInt(wallX * TileSize);

                    if (hit.side == HitAxis.X && rayDirX > 0) texX = TileSize - texX - 1;
                    if (hit.side == HitAxis.Y && rayDirY < 0) texX = TileSize - texX - 1;

                    for (int y = drawStart; y < drawEnd; y++)
                    {
                        float d = y - screenHeight * 0.5f + lineHeight * 0.5f;
                        int texY = MathUtils.FloorToInt(Math.Abs(((d * TileSize) / lineHeight)));

                        if (hit.cutOff > 0 && texY < hit.cutOff)
                        {
                            continue;
                        }

                        if (hit.cutOff < 0 && texY > -hit.cutOff)
                        {
                            continue;
                        }

                        byte red, green, blue, alpha;
                        float scale;
                        float depth;

                        if (hit.wallTex != null)
                        {
                            hit.wallTex.GetPixel(texX, texY, out red, out green, out blue, out alpha);
                            scale = CalculateFog(dist, hit.mapX, hit.mapY, texX, texY, false);

                            if (hit.side == HitAxis.Y)
                            {
                                //make color darker for y-sides
                                scale *= 0.5f;
                            }

                            depth = dist;
                        }
                        else
                        {
                            SampleSky(rayDirX, rayDirY, y, out red, out green, out blue, out alpha);
                            scale = 1.0f;

                            depth = 9998;
                        }

                        bool visible = alpha > 0;

                        if (visible)
                        {
                            WritePixel(x, y, texX, texY, red, green, blue, scale, depth);
                        }
                    }
                    drawEnd = tempEnd;

                    //SET THE ZBUFFER FOR THE SPRITE CASTING
                    ZBuffer[x] = perpWallDist; //perpendicular distance is used
                }

                //FLOOR CASTING
                float floorXWall, floorYWall; //x, y position of the floor texel at the bottom of the wall

                //4 different wall directions possible
                var bestHit = hits[hitCount-1];
                switch (bestHit.side)
                {
                    case HitAxis.X:
                        {
                            if (rayDirX > 0)
                            {
                                floorXWall = bestHit.mapX;
                            }
                            else
                            {
                                floorXWall = bestHit.mapX + 1.0f;
                            }
                            floorYWall = bestHit.mapY + wallX;
                            break;
                        }

                    default: // Y
                        {
                            if (rayDirY > 0)
                            {
                                floorXWall = bestHit.mapX + wallX;
                                floorYWall = bestHit.mapY;
                            }
                            else
                            {
                                floorXWall = bestHit.mapX + wallX;
                                floorYWall = bestHit.mapY + 1.0f;
                            }
                            break;
                        }
                }


                float distWall, distPlayer, currentDist;

                distWall = perpWallDist;
                distPlayer = 0.0f;

                //draw the floor from drawEnd to the bottom of the screen
                for (int y = drawEnd; y < Camera.maxY; y++)
                {
                    var temp = (2.0f * y) - screenHeight;
                    currentDist = screenHeight / temp; //you could make a small lookup table for this instead

                    float weight = (currentDist - distPlayer) / (distWall - distPlayer);

                    float currentFloorX = MathUtils.Lerp(Camera.posX, floorXWall, weight);
                    float currentFloorY = MathUtils.Lerp(Camera.posY, floorYWall, weight);

                    var mapX = MathUtils.FloorToInt(currentFloorX);
                    var mapY = MathUtils.FloorToInt(currentFloorY);

                    MapTile tile;
                    GetTileAt(mapX, mapY, out tile);

                    if (tile.wallID != 0 && !textures[tile.wallID].hasAlpha && tile.cutOff< HalfTileSize)
                    {
                        continue;
                    }

                    var dist = (currentDist - distPlayer);

                    byte red, green, blue, alpha;
                    float scale;
                    int floorTexX, floorTexY;

                    float depth;

                    if (tile.floorID > 0)
                    {
                        var floorTexture = textures[tile.floorID];
                        floorTexX = (int)(currentFloorX * TileSize) % floorTexture.Width;
                        floorTexY = (int)(currentFloorY * TileSize) % floorTexture.Height;

                        scale = CalculateFog(dist, mapX, mapY, floorTexX, floorTexY, false);
                        floorTexture.GetPixel(floorTexX, floorTexY, out red, out green, out blue, out alpha);

                        depth = dist;
                    }
                    else
                    {
                        scale = 1.0f;
                        floorTexX = 0;
                        floorTexY = 0;
                        depth = 9998;
                        SampleSky(rayDirX, rayDirY, y, out red, out green, out blue, out alpha);
                    }

                    int ofs = tile.cutOff <= 0 ? 0 : MathUtils.FloorToInt((screenHeight / dist) * (1.0f - (tile.cutOff / (float)TileSize)));
                    WritePixel(x, y - ofs, floorTexX, floorTexY, red, green, blue, scale, depth);
                }

                //ceiling
                for (int y = Camera.minY; y < drawStart; y++)
                {
                    var temp = (2.0f * (screenHeight- y)) - screenHeight;
                    currentDist = screenHeight / temp; //you could make a small lookup table for this instead

                    float weight = (currentDist - distPlayer) / (distWall - distPlayer);

                    float currentFloorX = MathUtils.Lerp(Camera.posX, floorXWall, weight);
                    float currentFloorY = MathUtils.Lerp(Camera.posY, floorYWall, weight);

                    var mapX = MathUtils.FloorToInt(currentFloorX);
                    var mapY = MathUtils.FloorToInt(currentFloorY);

                    MapTile tile;
                    GetTileAt(mapX, mapY, out tile);

                    int ceilTexX, ceilTexY;
                    float scale;

                    var dist = (currentDist - distPlayer);
                    byte red, green, blue, alpha;
                    float depth;

                    if (tile.ceilID > 0)
                    {
                        var ceilTexture = textures[tile.ceilID];
                        ceilTexX = (int)(currentFloorX * TileSize) % ceilTexture.Width;
                        ceilTexY = (int)(currentFloorY * TileSize) % ceilTexture.Height;
                        scale = CalculateFog(dist, mapX, mapY, ceilTexX, ceilTexY, false);

                        ceilTexture.GetPixel(ceilTexX, ceilTexY, out red, out green, out blue, out alpha);
                        depth = dist;
                    }
                    else
                    {
                        ceilTexX = 0;
                        ceilTexY = 0;
                        scale = 1;
                        SampleSky(rayDirX, rayDirY, y, out red, out green, out blue, out alpha);
                        depth = 9998;
                    }

                    WritePixel(x, y, ceilTexX, ceilTexY, red, green, blue, scale, depth);
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
                spriteQueue[i].distance = ((Camera.posX - sprite.posX) * (Camera.posX - sprite.posX) + (Camera.posY - sprite.posY) * (Camera.posY - sprite.posY)); //sqrt not taken, unneeded
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

        internal void WritePixel(int x, int y, int u, int v, byte red, byte green, byte blue, float scale, float dist)
        {
            y += Camera.drawOffset;

            if (y < 0 || y >= Output.Height)
            {
                return;
            }

            int zOfs = x + y * Output.Width;
            if (depthBuffer[zOfs] < dist)
            {
                return;
            }
            depthBuffer[zOfs] = dist;
            
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
