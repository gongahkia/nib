using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Summing.Core;
using Summing.Player;
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
    private const int MaximumHorizontalTiles = 6;
    private const int MaximumAscentTiles = 4;
    private const int MaximumDropTiles = 3;

    public static TraversabilityReport Validate(GeneratedWorld generated)
    {
        var anchors = generated.RouteAnchors;
        var obstructions = 0;
        if (anchors.Count < 2)
            return Failure("macro route has fewer than two anchors", 0, obstructions);

        var spawnBody = new Aabb(generated.Spawn.X - PlayerController.BodyWidth * 0.5f,
            generated.Spawn.Y - PlayerController.StandingBodyHeight, PlayerController.BodyWidth,
            PlayerController.StandingBodyHeight);
        if (generated.Terrain.OverlapsSolid(spawnBody))
            return Failure("one-tile player body overlaps terrain at spawn", 0, obstructions);

        var descents = 0;
        var lateralTransitions = 0;
        var directionChanges = 0;
        var previousDirection = 0;
        var horizontalTravel = 0;

        for (var index = 0; index < anchors.Count; index++)
        {
            var anchor = anchors[index];
            if (!generated.Terrain.Contains(anchor.X, anchor.Y))
                return Failure($"anchor {index} is outside the tile world", index, obstructions);
            if (!generated.Terrain.GetTile(anchor.X, anchor.Y).Solid)
                return Failure($"anchor {index} has no supporting terrain at {anchor.X},{anchor.Y} " +
                    $"({generated.Terrain.GetTile(anchor.X, anchor.Y).Material})", index, obstructions);
            var landingSupport = 0;
            for (var x = anchor.X - 1; x <= anchor.X + 1; x++)
                if (generated.Terrain.GetTile(x, anchor.Y).Solid) landingSupport++;
            if (landingSupport < 2)
                return Failure($"anchor {index} has less than two tiles of landing support", index, obstructions);
            var bodyY = anchor.Y - 1;
            var tile = generated.Terrain.GetTile(anchor.X, bodyY);
            if (tile.Solid)
            {
                var material = MaterialCatalog.Get(tile.Material);
                if (material.Hardness <= 0)
                    return Failure($"anchor {index} is blocked by non-diggable {material.Name}", index, obstructions);
                obstructions++;
            }
            if (index == anchors.Count - 1) continue;
            var next = anchors[index + 1];
            var horizontal = Math.Abs(next.X - anchor.X);
            var ascent = anchor.Y - next.Y;
            horizontalTravel += horizontal;
            if (ascent < 0) descents++;
            if (ascent <= 1 && horizontal >= 3) lateralTransitions++;
            var direction = Math.Sign(next.X - anchor.X);
            if (direction != 0 && previousDirection != 0 && direction != previousDirection) directionChanges++;
            if (direction != 0) previousDirection = direction;
            if (horizontal > MaximumHorizontalTiles)
                return Failure($"transition {index} spans {horizontal} horizontal tiles", index, obstructions);
            if (ascent > MaximumAscentTiles)
                return Failure($"transition {index} ascends {ascent} tiles", index, obstructions);
            if (ascent < -MaximumDropTiles)
                return Failure($"transition {index} drops {-ascent} tiles", index, obstructions);
        }

        if (descents < 2)
            return Failure("macro route lacks short downward diversions", anchors.Count - 1, obstructions);
        if (lateralTransitions < 6 || directionChanges < 5 || horizontalTravel < generated.Terrain.Height)
            return Failure("macro route lacks lateral travel or switchbacks", anchors.Count - 1, obstructions);

        var summit = generated.RouteAnchors[^1];
        var summitPoint = new Point((int)generated.SummitBounds.Center.X, (int)generated.SummitBounds.Center.Y);
        var summitTile = generated.Terrain.WorldToTile(summitPoint.ToVector2());
        if (Math.Abs(summit.X - summitTile.X) > 4 || Math.Abs(summit.Y - summitTile.Y) > 6)
            return Failure("summit trigger is detached from the validated ascent", anchors.Count - 1, obstructions);

        return new TraversabilityReport(true, anchors.Count - 1, obstructions, "",
            ["run", "jump", "coyote", "jump-buffer", "ledge-mantle", "wall-cling", "wall-jump", "dash", "digging-tool"]);
    }

    private static TraversabilityReport Failure(string failure, int transitions, int obstructions) =>
        new(false, transitions, obstructions, failure,
            ["run", "jump", "coyote", "jump-buffer", "ledge-mantle", "wall-cling", "wall-jump", "dash", "digging-tool"]);
}
