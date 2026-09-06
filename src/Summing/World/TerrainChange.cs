using Microsoft.Xna.Framework;
using Summing.World.Materials;

namespace Summing.World;

public sealed record TerrainChange(Point Tile, MaterialId Material, string Cause, int Damage, bool Destroyed);
