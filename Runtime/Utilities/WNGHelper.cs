using UnityEngine;

namespace Universe
{
    public static class WngHelper
    {
        public static float Brightness = 1.0f;
        public static float Contrast = 1.0f;
        public static int Octaves = 4;

        private static int Wrap(int n, int period)
        {
            return n >= 0 ? n % period : period + n;
        }

        public static float Noise(Vector3 pos, int period, int seed)
        {
            pos *= period;
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            int z = Mathf.FloorToInt(pos.z);
            Vector3 boxPos = new(x, y, z);
            float minDistance = float.MaxValue;

            for (int xoffset = -1; xoffset <= 1; xoffset++)
            {
                for (int yoffset = -1; yoffset <= 1; yoffset++)
                {
                    for (int zoffset = -1; zoffset <= 1; zoffset++)
                    {
                        Vector3 offset = new(xoffset, yoffset, zoffset);
                        Vector3 newboxPos = boxPos + offset;
                        int w1 = Wrap((int)newboxPos.x, period);
                        int w2 = Wrap((int)newboxPos.y, period);
                        int w3 = Wrap((int)newboxPos.z, period);
                        int hashValue = (w1 + w2 * 131 + w3 * 17161) % int.MaxValue;
                        Random.InitState(hashValue + seed);
                        float randomX = Random.value + newboxPos.x;
                        float randomY = Random.value + newboxPos.y;
                        float randomZ = Random.value + newboxPos.z;
                        Vector3 featurePoint = new(randomX, randomY, randomZ);
                        float distance = Vector3.Distance(pos, featurePoint);
                        minDistance = Mathf.Min(minDistance, distance);
                    }
                }
            }

            return 1.0f - minDistance;
        }

        public static float OctaveNoise(Vector3 pos, int octaves, int period, int seed = 0, float persistence = 0.5f)
        {
            float result = 0.0f;
            float amp = .5f;
            float freq = 1.0f;
            float totalAmp = 0.0f;
            for (int i = 0; i < octaves; i++)
            {
                totalAmp += amp;
                int fp = Mathf.RoundToInt(freq * period);
                float noise = Noise(pos, fp, seed);
                result += noise * amp;
                amp *= persistence;
                freq /= persistence;
            }
            
            return octaves == 0 ? 0.0f : result / totalAmp;
        }

        public static float ModifiedOctaveNoise(Vector3 pos, int octaves, int period, int seed = 0, float persistence = 0.5f, float fade = 0.0f)
        {
            float result = fade;
            float amp = .5f;
            float freq = 1.0f;
            float totalAmp = 0.0f;
            for (int i = 0; i < octaves; i++)
            {
                totalAmp += amp;
                int fp = Mathf.RoundToInt(freq * period);
                float noise = Noise(pos, fp, seed);
                result += noise * amp;
                amp *= persistence;
                freq /= persistence;
            }
            
            return octaves == 0 ?  0.0f : result / totalAmp;
        }
    }
}