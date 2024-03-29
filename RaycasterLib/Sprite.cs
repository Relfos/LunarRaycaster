﻿namespace LunarLabs.Raycaster
{
    public class Sprite
    {
        public float posX;
        public float posY;
        public int textureID;
        public int vOffset; // negative numbers make the sprites float up, positive make them go under floor
        public bool isEmissive = false;
        public int angleFrames = 1;
        private float rotation = 0;

        public Sprite(float x, float y, int textureID, int vOffset = 0, int angleFrames = 1, float angle = 0)
        {
            this.posX = x;
            this.posY = y;
            this.textureID = textureID;
            this.vOffset = vOffset;
            this.angleFrames = angleFrames;
            this.rotation = angle;
        }

        public void Render(Raycaster raycaster, float invDet)
        {
            int screenWidth = raycaster.Output.Width;
            int screenHeight = raycaster.Output.Height;

            //translate sprite position to relative to camera

            var cam = raycaster.Camera;

            float spriteX = this.posX - cam.posX;
            float spriteY = this.posY - cam.posY;

            //transform sprite with the inverse camera matrix
            // [ planeX   dirX ] -1                                       [ dirY      -dirX ]
            // [               ]       =  1/(planeX*dirY-dirX*planeY) *   [                 ]
            // [ planeY   dirY ]                                          [ -planeY  planeX ]

            float transformX = invDet * (cam.dirY * spriteX - cam.dirX * spriteY);
            float transformY = invDet * (-cam.planeY * spriteX + cam.planeX * spriteY); //this is actually the depth inside the screen, that what Z is in 3D

            float spriteScreenX = (screenWidth / 2) * (1 + transformX / transformY);

            float vMoveScreen = this.vOffset / transformY;

            //calculate height of the sprite on screen
            float spriteHeight  = screenHeight / transformY;//using "transformY" instead of the real distance prevents fisheye

            //calculate width of the sprite
            float spriteWidth  = screenHeight / transformY;

            int mapX = Mathf.FloorToInt(posX);
            int mapY = Mathf.FloorToInt(posY);

            int drawStartX = Mathf.FloorToInt(-spriteWidth / 2 + spriteScreenX);
            int drawEndX = Mathf.FloorToInt(spriteWidth / 2 + spriteScreenX);

            if (drawStartX < 0) drawStartX = 0;
            if (drawEndX >= screenWidth) drawEndX = screenWidth - 1;

            //calculate lowest and highest pixel to fill in current stripe
            float spriteScreenY = screenHeight / 2 + vMoveScreen;
            int drawStartY = Mathf.FloorToInt(-spriteHeight / 2 + spriteScreenY);
            int drawEndY = Mathf.FloorToInt(spriteHeight / 2 + spriteScreenY);

            if (drawStartY < cam.minY) drawStartY = cam.minY;
            if (drawEndY > cam.maxY) drawEndY = cam.maxY;

            int spriteID = this.textureID;

            if (angleFrames > 1)
            {
                float theta = Mathf.Atan2(spriteX, spriteY) * Mathf.Rad2Deg;
                theta += this.rotation;

                while (theta < 0.0f) theta += 360;
                while (theta >= 360.0f) theta -= 360;

                float delta = 360.0f / (float)angleFrames;
                spriteID += Mathf.FloorToInt(theta / delta);
            }

            var texture = raycaster.textures[spriteID];

            //loop through every vertical stripe of the sprite on screen
            for (int stripe = drawStartX; stripe < drawEndX; stripe++)
            {
                int texX = Mathf.FloorToInt((stripe - (-spriteWidth / 2 + spriteScreenX)) * texture.Width / spriteWidth);
                //the conditions in the if are:
                //1) it's in front of camera plane so you don't see things behind you
                //2) it's on the screen (left)
                //3) it's on the screen (right)
                //4) ZBuffer, with perpendicular distance
                if (transformY > 0 && stripe > 0 && stripe < screenWidth && transformY < raycaster.ZBuffer[stripe])
                {
                    float scale = raycaster.CalculateFog(transformY, mapX, mapY, texX, texX, isEmissive);

                    for (int y = drawStartY; y < drawEndY; y++) //for every pixel of the current stripe
                    {
                        float d = (y - vMoveScreen) - screenHeight * 0.5f + spriteHeight * 0.5f;
                        int texY =Mathf.FloorToInt( ((d * texture.Height) / spriteHeight));

                        byte red, green, blue, alpha;

                        texture.GetPixel(texX, texY, out red, out green, out blue, out alpha); //get current color from the texture

                        if (alpha == 0)
                        {
                            continue;
                        }

                        /*
                        if (alpha < 1 && !DitherUtils.ColorDither(DitherMode.Bayer4x4, stripe, y, alpha))
                        {
                            continue;
                        }*/

                        raycaster.WritePixel(stripe, y, texX, texY, red, green, blue, scale, transformY);
                    }
                }

            }
        }
    };
}
