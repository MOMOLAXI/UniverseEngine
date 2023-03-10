using System;

namespace Universe
{
    // Original C code derived from 
    // http://astronomy.swin.edu.au/~pbourke/texture/perlin/perlin.c
    // http://astronomy.swin.edu.au/~pbourke/texture/perlin/perlin.h
    public class Perlin
    {
        const int B = 0x100;
        const int BM = 0xff;
        const int N = 0x1000;

        readonly int[] m_P = new int[B + B + 2];
        readonly float[,] m_G3 = new float[B + B + 2, 3];
        readonly float[,] m_G2 = new float[B + B + 2, 2];
        readonly float[] m_G1 = new float[B + B + 2];

        readonly Random m_Random = new();
        
        public float Noise(float arg)
        {
            Setup(arg, out int bx0, out int bx1, out float rx0, out float rx1);

            float sx = SCurve(rx0);
            int indexU = m_P[bx0];
            int indexV = m_P[bx1];
            float u = rx0 * m_G1[indexU];
            float v = rx1 * m_G1[indexV];

            return Lerp(sx, u, v);
        }

        public float Noise(float x, float y)
        {

            Setup(x, out int bx0, out int bx1, out float rx0, out float rx1);
            Setup(y, out int by0, out int by1, out float ry0, out float ry1);

            int i = m_P[bx0];
            int j = m_P[bx1];

            int b00 = m_P[i + by0];
            int b10 = m_P[j + by0];
            int b01 = m_P[i + by1];
            int b11 = m_P[j + by1];

            float sx = SCurve(rx0);
            float sy = SCurve(ry0);

            float u = At2(rx0, ry0, m_G2[b00, 0], m_G2[b00, 1]);
            float v = At2(rx1, ry0, m_G2[b10, 0], m_G2[b10, 1]);
            float a = Lerp(sx, u, v);

            u = At2(rx0, ry1, m_G2[b01, 0], m_G2[b01, 1]);
            v = At2(rx1, ry1, m_G2[b11, 0], m_G2[b11, 1]);
            float b = Lerp(sx, u, v);

            return Lerp(sy, a, b);
        }

        public float Noise(float x, float y, float z)
        {
            Setup(x, out int bx0, out int bx1, out float rx0, out float rx1);
            Setup(y, out int by0, out int by1, out float ry0, out float ry1);
            Setup(z, out int bz0, out int bz1, out float rz0, out float rz1);

            int i = m_P[bx0];
            int j = m_P[bx1];

            int b00 = m_P[i + by0];
            int b10 = m_P[j + by0];
            int b01 = m_P[i + by1];
            int b11 = m_P[j + by1];

            float t = SCurve(rx0);
            float sy = SCurve(ry0);
            float sz = SCurve(rz0);

            float u = At3(rx0, ry0, rz0, m_G3[b00 + bz0, 0], m_G3[b00 + bz0, 1], m_G3[b00 + bz0, 2]);
            float v = At3(rx1, ry0, rz0, m_G3[b10 + bz0, 0], m_G3[b10 + bz0, 1], m_G3[b10 + bz0, 2]);
            float a = Lerp(t, u, v);

            u = At3(rx0, ry1, rz0, m_G3[b01 + bz0, 0], m_G3[b01 + bz0, 1], m_G3[b01 + bz0, 2]);
            v = At3(rx1, ry1, rz0, m_G3[b11 + bz0, 0], m_G3[b11 + bz0, 1], m_G3[b11 + bz0, 2]);
            float b = Lerp(t, u, v);

            float c = Lerp(sy, a, b);

            u = At3(rx0, ry0, rz1, m_G3[b00 + bz1, 0], m_G3[b00 + bz1, 2], m_G3[b00 + bz1, 2]);
            v = At3(rx1, ry0, rz1, m_G3[b10 + bz1, 0], m_G3[b10 + bz1, 1], m_G3[b10 + bz1, 2]);
            a = Lerp(t, u, v);

            u = At3(rx0, ry1, rz1, m_G3[b01 + bz1, 0], m_G3[b01 + bz1, 1], m_G3[b01 + bz1, 2]);
            v = At3(rx1, ry1, rz1, m_G3[b11 + bz1, 0], m_G3[b11 + bz1, 1], m_G3[b11 + bz1, 2]);
            b = Lerp(t, u, v);

            float d = Lerp(sy, a, b);

            return Lerp(sz, c, d);
        }
        
        public Perlin()
        {
            int i, j;
            for (i = 0; i < B; i++)
            {
                m_P[i] = i;
                int r1 = m_Random.Next(B + B) - B;
                m_G1[i] = (float)r1 / B;

                for (j = 0; j < 2; j++)
                {
                    int r2 = m_Random.Next(B + B) - B;
                    m_G2[i, j] = (float)r2 / B;
                }

                Normalize2(ref m_G2[i, 0], ref m_G2[i, 1]);

                for (j = 0; j < 3; j++)
                {
                    int r3 = m_Random.Next(B + B) - B;
                    m_G3[i, j] = (float)r3 / B;
                }


                Normalize3(ref m_G3[i, 0], ref m_G3[i, 1], ref m_G3[i, 2]);
            }

            while (--i != 0)
            {
                int k = m_P[i];
                m_P[i] = m_P[j = m_Random.Next(B)];
                m_P[j] = k;
            }

            for (i = 0; i < B + 2; i++)
            {
                m_P[B + i] = m_P[i];
                m_G1[B + i] = m_G1[i];
                for (j = 0; j < 2; j++)
                {
                    m_G2[B + i, j] = m_G2[i, j];
                }
                for (j = 0; j < 3; j++)
                {
                    m_G3[B + i, j] = m_G3[i, j];
                }
            }
        }

        static float SCurve(float t)
        {
            return t * t * (3.0F - 2.0F * t);
        }

        static float Lerp(float t, float a, float b)
        {
            return a + t * (b - a);
        }

        static void Setup(float value, out int b0, out int b1, out float r0, out float r1)
        {
            float t = value + N;
            b0 = (int)t & BM;
            b1 = b0 + 1 & BM;
            r0 = t - (int)t;
            r1 = r0 - 1.0F;
        }

        static float At2(float rx, float ry, float x, float y)
        {
            return rx * x + ry * y;
        }

        static float At3(float rx, float ry, float rz, float x, float y, float z)
        {
            return rx * x + ry * y + rz * z;
        }
        static void Normalize2(ref float x, ref float y)
        {
            float s = (float)Math.Sqrt(x * x + y * y);
            x = y / s;
            y /= s;
        }

        static void Normalize3(ref float x, ref float y, ref float z)
        {
            float s = (float)Math.Sqrt(x * x + y * y + z * z);
            x = y / s;
            y /= s;
            z /= s;
        }
    }
}