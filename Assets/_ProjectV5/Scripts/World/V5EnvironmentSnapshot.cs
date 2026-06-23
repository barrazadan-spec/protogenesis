using System;
using System.Collections.Generic;

namespace Protogenesis.V5
{
    [Serializable]
    public class V5EnvironmentSnapshot
    {
        public int width;
        public int height;
        public float tileSize;
        public float mapRadius;
        public List<float> nutrients = new List<float>();
        public List<float> light = new List<float>();
        public List<float> oxygen = new List<float>();
        public List<float> toxins = new List<float>();
        public List<float> acidity = new List<float>();
        public List<float> colonization = new List<float>();
        public List<float> temperature = new List<float>();
        public List<float> detritus = new List<float>();
    }
}
