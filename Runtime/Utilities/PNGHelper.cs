using UnityEngine;

namespace Universe
{
    public static class PNGHelper
    {
        static readonly int[] Perm =
        {
            151, 160, 137, 91, 90, 15,
            131, 13, 201, 95, 96, 53,
            194, 233, 7, 225, 140, 36,
            103, 30, 69, 142, 8, 99,
            37, 240, 21, 10, 23, 190,
            6, 148, 247, 120, 234, 75,
            0, 26, 197, 62, 94, 252,
            219, 203, 117, 35, 11, 32,
            7, 177, 33, 88, 237, 149,
            56, 87, 174, 20, 125, 136,
            171, 168, 68, 175, 74, 165,
            71, 134, 139, 48, 27, 166,
            77, 146, 158, 231, 83, 111,
            229, 122, 60, 211, 133, 230,
            220, 105, 92, 41, 55, 46,
            245, 40, 244, 102, 143, 54,
            65, 25, 63, 161, 1, 216,
            80, 73, 209, 76, 132, 187,
            208, 89, 18, 169, 200, 196,
            135, 130, 116, 188, 159, 86,
            164, 100, 109, 198, 173, 186,
            3, 64, 52, 217, 226, 250,
            124, 123, 5, 202, 38, 147,
            118, 126, 255, 82, 85, 212,
            207, 206, 59, 227, 47, 16,
            58, 17, 182, 189, 28, 42,
            223, 183, 170, 213, 119, 248,
            152, 2, 44, 154, 163, 70,
            221, 153, 101, 155, 167, 43,
            172, 9, 129, 22, 39, 253,
            19, 98, 108, 110, 79, 113,
            224, 232, 178, 185, 112, 104,
            218, 246, 97, 228, 251, 34,
            242, 193, 238, 210, 144, 12,
            191, 179, 162, 241, 81, 51,
            145, 235, 249, 14, 239, 107,
            49, 192, 214, 31, 181, 199,
            106, 157, 184, 84, 204, 176,
            115, 121, 50, 45, 127, 4,
            150, 254, 138, 236, 205, 93,
            222, 114, 67, 29, 24, 72,
            243, 141, 128, 195, 78, 66,
            215, 61, 156, 180, 151
        };
        static int Wrap(int n, int period)
        {
            n++;
            return period > 0 ? n % period : n;
        }

        public static float Noise(Vector3 pos, int period)
        {
            pos *= period;
            float x = pos.x;
            float y = pos.y;
            float z = pos.z;
            int X = Mathf.FloorToInt(x) & 0xff;
            int Y = Mathf.FloorToInt(y) & 0xff;
            int Z = Mathf.FloorToInt(z) & 0xff;
            x -= Mathf.Floor(x);
            y -= Mathf.Floor(y);
            z -= Mathf.Floor(z);

            float u = Fade(x);
            float v = Fade(y);
            float w = Fade(z);

            int wA = Wrap(X, period);
            int wB = Wrap(X + 1, period);
            int A = Perm[wA] + Y & 0xff;
            int B = Perm[wB] + Y & 0xff;

            int wAA = Wrap(A, period);
            int wBA = Wrap(B, period);
            int wAB = Wrap(A + 1, period);
            int wBB = Wrap(B + 1, period);
            int AA = Perm[wAA] + Z & 0xff;
            int BA = Perm[wBA] + Z & 0xff;
            int AB = Perm[wAB] + Z & 0xff;
            int BB = Perm[wBB] + Z & 0xff;

            float aa2ab = Lerp(u, Grad(Perm[AA], x, y, z), Grad(Perm[BA], x - 1, y, z));
            float ab2bb = Lerp(u, Grad(Perm[AB], x, y - 1, z), Grad(Perm[BB], x - 1, y - 1, z));
            float aa2baOff1 = Lerp(u, Grad(Perm[AA + 1], x, y, z - 1), Grad(Perm[BA + 1], x - 1, y, z - 1));
            float ab2bbOff1 = Lerp(u, Grad(Perm[AB + 1], x, y - 1, z - 1), Grad(Perm[BB + 1], x - 1, y - 1, z - 1));
            float result = Lerp(w, Lerp(v, aa2ab, ab2bb), Lerp(v, aa2baOff1, ab2bbOff1));
            return (result + 1.0f) / 2.0f;
        }

        public static float OctaveNoise(Vector3 pos, int period, int octaves, float persistence = 0.5f)
        {
            float total = 0.0f;
            float result = 0.0f;
            float amp = .5f;
            float freq = 1.0f;
            for (int i = 0; i < octaves; i++)
            {
                total += amp;
                result += (Noise(pos, Mathf.RoundToInt(freq * period)) * 2.0f - 1.0f) * amp;
                amp *= persistence;
                freq *= 2.0f;
            }

            return octaves == 0 ? 0.0f : (result / total + 1.0f) / 2.0f;
        }

        static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        static float Lerp(float t, float a, float b)
        {
            return a + t * (b - a);
        }

        static float Grad(int hash, float x, float y, float z)
        {
            int h = hash & 15;
            float u = h < 8 ? x : y;
            float v = h < 4 ? y : h == 12 || h == 14 ? x : z;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }
    }
}