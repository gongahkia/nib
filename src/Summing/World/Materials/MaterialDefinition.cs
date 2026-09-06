using Microsoft.Xna.Framework;

namespace Summing.World.Materials;

public sealed record MaterialDefinition(
    MaterialId Id,
    string Name,
    bool Solid,
    int Hardness,
    float Friction,
    float Brittleness,
    float Heat,
    float Conductivity,
    bool Climbable,
    bool Structural,
    Color BaseColor,
    Color AccentColor);
