using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Summing.World.Materials;

namespace Summing.Generation;

public sealed record TraversabilityReport(
    bool Solvable,
    int CheckedTransitions,
    int DiggableObstructions,
    string Failure,
    IReadOnlyList<string> AllowedResources);

public static class TraversabilityValidator
{
    private const int MaximumHorizontalTiles = 9;
    private const int MaximumAscentTiles = 5;
    private const int MaximumDropTiles = 8;

    public static TraversabilityReport Validate(GeneratedWorld generated)
    {
        var anchors = generated.RouteAnchors;
        var obstructions = 0;
        if (anchors.Count < 2)
            return Failure("macro route has fewer than two anchors", 0, obstructions);

        for (var index = 0; index < anchors.Count; index++)
        {
            var anchor = anchors[index];
            if (!generated.Terrain.GetTile(anchor.X, anchor.Y).Solid)
                return Failure($"anchor {index} has no supporting terrain at {anchor.X},{anchor.Y}", index, obstructions);
            for (var bodyY = anchor.Y - 2; bodyY < anchor.Y; bodyY++)
            {
                var tile = generated.Terrain.GetTile(anchor.X, bodyY);
                if (!tile.Solid) continue;
                var material = MaterialCatalog.Get(tile.Material);
                if (material.Hardness <= 0)
                    return Failure($"anchor {index} is blocked by non-diggable {material.Name}", index, obstructions);
                obstructions++;
            }
            if (index == anchors.Count - 1) continue;
            var next = anchors[index + 1];
            var horizontal = Math.Abs(next.X - anchor.X);
            var ascent = anchor.Y - next.Y;
            if (horizontal > MaximumHorizontalTiles)
                return Failure($"transition {index} spans {horizontal} horizontal tiles", index, obstructions);
            if (ascent > MaximumAscentTiles)
                return Failure($"transition {index} ascends {ascent} tiles", index, obstructions);
            if (ascent < -MaximumDropTiles)
                return Failure($"transition {index} drops {-ascent} tiles", index, obstructions);
        }

        var summit = generated.RouteAnchors[^1];
        var summitPoint = new Point((int)generated.SummitBounds.Center.X, (int)generated.SummitBounds.Center.Y);
        var summitTile = generated.Terrain.WorldToTile(summitPoint.ToVector2());
        if (Math.Abs(summit.X - summitTile.X) > 4 || Math.Abs(summit.Y - summitTile.Y) > 6)
            return Failure("summit trigger is detached from the validated ascent", anchors.Count - 1, obstructions);

        return new TraversabilityReport(true, anchors.Count - 1, obstructions, "",
            ["run", "jump", "coyote", "wall-cling", "wall-jump", "dash", "grapple", "digging-tool"]);
    }

    private static TraversabilityReport Failure(string failure, int transitions, int obstructions) =>
        new(false, transitions, obstructions, failure,
            ["run", "jump", "coyote", "wall-cling", "wall-jump", "dash", "grapple", "digging-tool"]);
}
