using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PerlinNoise {
    private int[] permutations;

    public PerlinNoise(int seed) {
        var random = new System.Random(seed);
        permutations = new int[512];
        int[] p = new int[256];

        for (int i = 0; i < 256; i++) {
            p[i] = i;
        }

        for (int i = 255; i > 0; i--) {
            int n = random.Next(i + 1);
            int temp = p[i];
            p[i] = p[n];
            p[n] = temp;
        }

        for (int i = 0; i < 512; i++) {
            permutations[i] = p[i & 255];
        }
    }

    private float Fade(float t) {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private float Lerp(float t, float a, float b) {
        return a + t * (b - a);
    }

    private float Grad(int hash, float x, float y, float z) {
        int h = hash & 15;
        float u = h < 8 ? x : y;
        float v = h < 4 ? y : h == 12 || h == 14 ? x : z;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    public float Noise(float x, float y, float z) {
        int X = (int)Math.Floor(x) & 255;
        int Y = (int)Math.Floor(y) & 255;
        int Z = (int)Math.Floor(z) & 255;
        x -= (float)Math.Floor(x);
        y -= (float)Math.Floor(y);
        z -= (float)Math.Floor(z);
        float u = Fade(x);
        float v = Fade(y);
        float w = Fade(z);
        int A = permutations[X] + Y;
        int AA = permutations[A] + Z;
        int AB = permutations[A + 1] + Z;
        int B = permutations[X + 1] + Y;
        int BA = permutations[B] + Z;
        int BB = permutations[B + 1] + Z;

        return Lerp(w, Lerp(v, Lerp(u, Grad(permutations[AA], x, y, z),
                                       Grad(permutations[BA], x - 1, y, z)),
                               Lerp(u, Grad(permutations[AB], x, y - 1, z),
                                       Grad(permutations[BB], x - 1, y - 1, z))),
                       Lerp(v, Lerp(u, Grad(permutations[AA + 1], x, y, z - 1),
                                       Grad(permutations[BA + 1], x - 1, y, z - 1)),
                               Lerp(u, Grad(permutations[AB + 1], x, y - 1, z - 1),
                                       Grad(permutations[BB + 1], x - 1, y - 1, z - 1))));
    }
}
