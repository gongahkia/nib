using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Summing.Core;
using Summing.Entities;
using Summing.Gameplay;
using Summing.Generation;
using Summing.History;
using Summing.Input;
using Summing.Player;
using Summing.World;
using Summing.World.Materials;

namespace Summing.Diagnostics;

public static class HeadlessVerification
{
    public static void RunSystems()
    {
        VerifyMovementTransitions();
        VerifyMaterialHardness();
        VerifyBombAndBurrowerMutation();
        VerifyBrittleAndArchive();
        VerifyInputBindingPersistence();
        Console.WriteLine("systems=ok movement=jump-dash material=hardness terrain=bomb-burrower hazard=brittle archive=persistent-json input=remap-json");
    }

    private static void VerifyMovementTransitions()
    {
        var world = TileWorld.CreateMovementTest();
        var player = new PlayerController(new Vector2(7f * GameConstants.TileSize, 15f * GameConstants.TileSize));
        var input = new InputManager(new InputBindings());
        for (var frame = 0; frame < 3; frame++)
        {
            input.SetSyntheticState(Vector2.Zero, Vector2.UnitX);
            player.Update(input, world, GameConstants.FixedDelta);
        }
        var start = player.Position;
        input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right, InputAction.Jump);
        player.Update(input, world, GameConstants.FixedDelta);
        if (player.Velocity.Y >= 0f) throw new InvalidOperationException("scripted jump did not begin");
        for (var frame = 0; frame < 16; frame++)
        {
            input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right, InputAction.Jump);
            player.Update(input, world, GameConstants.FixedDelta);
        }
        input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right);
        player.Update(input, world, GameConstants.FixedDelta);
        input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right, InputAction.Dash);
        player.Update(input, world, GameConstants.FixedDelta);
        if (player.DashCharges != 0 || player.VisualState != MovementState.Dash)
            throw new InvalidOperationException("scripted air dash did not consume its charge");
        if (player.Position.X <= start.X) throw new InvalidOperationException("scripted movement did not advance");
    }

    private static void VerifyMaterialHardness()
    {
        var world = new TileWorld(8, 8);
        world.SetTile(3, 3, MaterialId.BlackBasalt);
        var hardness = MaterialCatalog.Get(MaterialId.BlackBasalt).Hardness;
        for (var strike = 1; strike < hardness; strike++)
        {
            if (world.DamageTile(3, 3, 1, "verification"))
                throw new InvalidOperationException("hard material broke before reaching hardness");
        }
        if (!world.DamageTile(3, 3, 1, "verification") || world.GetTile(3, 3).Solid)
            throw new InvalidOperationException("hard material did not break at hardness");
    }

    private static void VerifyBombAndBurrowerMutation()
    {
        var world = new TileWorld(28, 18);
        for (var x = 2; x < 26; x++)
            for (var y = 9; y < 14; y++) world.SetTile(x, y, MaterialId.RedSandstone);
        var beforeBomb = world.Fingerprint();
        var bomb = new BombEntity(new Vector2(12 * GameConstants.TileSize, 8 * GameConstants.TileSize), Vector2.Zero);
        for (var frame = 0; frame < 180 && bomb.Alive; frame++)
            if (bomb.Update(world, GameConstants.FixedDelta)) world.DamageCircle(bomb.Position, 72f, 8, "bomb");
        if (world.Fingerprint() == beforeBomb) throw new InvalidOperationException("bomb did not mutate terrain");

        for (var x = 4; x < 12; x++) world.SetTile(x, 5, MaterialId.Loess);
        var feature = new WorldFeature(WorldFeatureKind.BurrowerSpawn,
            new Vector2(4.5f * GameConstants.TileSize, 5.5f * GameConstants.TileSize), 0);
        var burrower = new BurrowerEntity(feature);
        var player = new PlayerController(new Vector2(14f * GameConstants.TileSize, 6f * GameConstants.TileSize));
        var beforeBurrow = world.Fingerprint();
        for (var frame = 0; frame < 240; frame++) burrower.Update(player, world, 1f, 1, GameConstants.FixedDelta);
        if (world.Fingerprint() == beforeBurrow) throw new InvalidOperationException("burrower did not mutate terrain");
    }

    private static void VerifyBrittleAndArchive()
    {
        var generated = ValidatedWorldGenerator.Generate(new WorldGenerationConfig());
        var tuning = DifficultyTuning.For(Difficulty.Easy);
        var brittle = new BrittleSystem(generated, tuning);
        if (brittle.Structures.Count == 0) throw new InvalidOperationException("generation placed no brittle structure");
        var structure = brittle.Structures[0];
        var support = structure.Tiles[0];
        var player = new PlayerController(new Vector2((support.X + 0.5f) * GameConstants.TileSize, support.Y * GameConstants.TileSize));
        for (var frame = 0; frame < 120; frame++) brittle.Update(player, generated.Terrain, GameConstants.FixedDelta);
        if (!structure.Collapsed) throw new InvalidOperationException("brittle structure did not collapse under player");

        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"summing-verify-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var archivePath = Path.Combine(temporaryDirectory, "archive.json");
            var archive = new ArchiveStore(archivePath);
            var relics = new RelicSystem(generated, archive);
            if (relics.Relics.Count == 0) throw new InvalidOperationException("generation placed no relic");
            player.Teleport(relics.Relics[0].Position);
            relics.Update(player, GameConstants.FixedDelta);
            if (archive.Discoveries.Count != 1 || !File.Exists(archivePath))
                throw new InvalidOperationException("relic did not persist to archive JSON");
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    private static void VerifyInputBindingPersistence()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"summing-bindings-{Guid.NewGuid():N}");
        var path = Path.Combine(temporaryDirectory, "bindings.json");
        try
        {
            var bindings = InputBindings.LoadOrCreate(path);
            bindings.Rebind(InputAction.Jump, Keys.Z, Buttons.B);
            bindings.Save(path);
            var loaded = InputBindings.LoadOrCreate(path);
            var jump = loaded.Actions[InputAction.Jump];
            if (jump.Key != Keys.Z || jump.Button != Buttons.B)
                throw new InvalidOperationException("remapped input did not persist to JSON");
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory)) Directory.Delete(temporaryDirectory, true);
        }
    }
}
