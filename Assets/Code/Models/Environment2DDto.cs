using System;

namespace Assets.Code.Models
{
    [Serializable]
    public class Environment2DDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int MaxHeight { get; set; }

        public int MaxLength { get; set; }
    }
}