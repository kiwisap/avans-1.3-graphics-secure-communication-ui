using System;

namespace Assets.Code.Models
{
    [Serializable]
    public class Environment2DDto
    {
        public int id { get; set; }

        public string name { get; set; }

        public int maxHeight { get; set; }

        public int maxLength { get; set; }
    }
}