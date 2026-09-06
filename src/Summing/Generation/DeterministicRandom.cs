using System;

namespace Summing.Generation;

public sealed class DeterministicRandom
{
    private ulong _state;

    public DeterministicRandom(long seed)
    {
        _state = Mix(unchecked((ulong)seed)) | 1UL;
    }

    public ulong NextU64()
    {
        _state ^= _state >> 12;
        _state ^= _state << 25;
        _state ^= _state >> 27;
        return _state * 2685821657736338717UL;
    }

    public int Range(int minimumInclusive, int maximumExclusive)
    {
        if (maximumExclusive <= minimumInclusive) return minimumInclusive;
        return minimumInclusive + (int)(NextU64() % (ulong)(maximumExclusive - minimumInclusive));
    }

    public float NextFloat() => (NextU64() >> 40) / (float)(1UL << 24);
    public bool Chance(float probability) => NextFloat() < Math.Clamp(probability, 0f, 1f);

    public static uint Hash(long seed, int x, int y, int channel = 0)
    {
        var value = unchecked((ulong)seed) ^ ((ulong)(uint)x << 32) ^ (uint)y ^ ((ulong)(uint)channel * 0x9e3779b9UL);
        return (uint)(Mix(value) >> 32);
    }

    public static float Hash01(long seed, int x, int y, int channel = 0) => Hash(seed, x, y, channel) / (float)uint.MaxValue;

    private static ulong Mix(ulong value)
    {
        value += 0x9e3779b97f4a7c15UL;
        value = (value ^ (value >> 30)) * 0xbf58476d1ce4e5b9UL;
        value = (value ^ (value >> 27)) * 0x94d049bb133111ebUL;
        return value ^ (value >> 31);
    }
}
