using System;

namespace Assets.Code.Models
{
    [Serializable]
    public class Object2DDto
    {
        public int Id { get; set; }

        public int EnvironmentId { get; set; }

        public int PrefabId { get; set; }

        public float PositionX { get; set; }

        public float PositionY { get; set; }

        public float ScaleX { get; set; }

        public float ScaleY { get; set; }

        public float RotationZ { get; set; }

        public int SortingLayer { get; set; }
    }
}