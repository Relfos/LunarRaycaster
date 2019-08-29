﻿namespace LunarLabs.Raycaster
{
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
            int limit = Raycaster.TileSize;
            if (lookOffset < -limit) lookOffset = -limit;
            if (lookOffset > limit) lookOffset = limit;

            drawOffset = Mathf.FloorToInt(lookOffset);
            minY = -drawOffset;
            maxY = (raycaster.Output.Height - 1) - drawOffset;
        }

        public void Rotate(float rotSpeed)
        {
            float oldDirX = dirX;
            dirX = dirX * Mathf.Cos(rotSpeed) - dirY * Mathf.Sin(rotSpeed);
            dirY = oldDirX * Mathf.Sin(rotSpeed) + dirY * Mathf.Cos(rotSpeed);
            float oldPlaneX = planeX;
            planeX = planeX * Mathf.Cos(rotSpeed) - planeY * Mathf.Sin(rotSpeed);
            planeY = oldPlaneX * Mathf.Sin(rotSpeed) + planeY * Mathf.Cos(rotSpeed);
        }
    }
}
