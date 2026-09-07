using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Summing.Camera;
using Summing.Core;
using Summing.Entities;
using Summing.Gameplay;
using Summing.Generation;
using Summing.History;
using Summing.Input;
using Summing.Player;
using Summing.Rendering;
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
        VerifyImpactFeedback();
        Console.WriteLine("systems=ok movement=one-tile-jump-dash fall=one-heart camera=zoomed-out material=hardness terrain=bomb-burrower hazard=brittle archive=persistent-json input=remap-json feedback=shake-debris");
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
        if (player.Bounds.Height > GameConstants.TileSize || player.Bounds.Width > GameConstants.TileSize)
            throw new InvalidOperationException("player body is larger than one authoritative tile");
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

        VerifyCrouchAndSlideClearance();
        VerifyCoyoteAndJumpBuffer();
        VerifyDashStopsAtTerrain();
        VerifyWallAndLedgeTransitions();
        VerifyToolReachStopsAtAdjacentTile();
        VerifyFallDamageTuning();
    }

    private static void VerifyCrouchAndSlideClearance()
    {
        var world = new TileWorld(18, 12);
        for (var x = 0; x < world.Width; x++) world.SetTile(x, 9, MaterialId.RedSandstone);
        var player = new PlayerController(new Vector2(5.5f * GameConstants.TileSize, 9f * GameConstants.TileSize));
        var input = new InputManager(new InputBindings());
        for (var frame = 0; frame < 8; frame++)
        {
            input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right);
            player.Update(input, world, GameConstants.FixedDelta);
        }
        input.SetSyntheticState(new Vector2(1f, 1f), Vector2.UnitX, InputAction.Right, InputAction.Down, InputAction.Slide);
        player.Update(input, world, GameConstants.FixedDelta);
        if (player.Bounds.Height != PlayerController.CrouchingBodyHeight || player.VisualState != MovementState.Slide)
            throw new InvalidOperationException("slide did not use the one-tile player's short collision body");

        var lowWorld = new TileWorld(12, 12);
        for (var x = 2; x < 10; x++) lowWorld.SetTile(x, 5, MaterialId.BlackBasalt);
        player.Teleport(new Vector2(5.5f * GameConstants.TileSize, 5f * GameConstants.TileSize + PlayerController.CrouchingBodyHeight));
        input.SetSyntheticState(Vector2.UnitY, Vector2.UnitX, InputAction.Down);
        player.Update(input, lowWorld, GameConstants.FixedDelta);
        input.SetSyntheticState(Vector2.Zero, Vector2.UnitX);
        player.Update(input, lowWorld, GameConstants.FixedDelta);
        if (player.Bounds.Height != PlayerController.CrouchingBodyHeight)
            throw new InvalidOperationException("player stood up through a low ceiling");
        player.Reset(new Vector2(4.5f * GameConstants.TileSize, 9f * GameConstants.TileSize));
        if (player.Bounds.Height != PlayerController.StandingBodyHeight || player.State != MovementState.Falling)
            throw new InvalidOperationException("run reset retained a stale crouch or movement state");
    }

    private static void VerifyCoyoteAndJumpBuffer()
    {
        var world = new TileWorld(18, 14);
        for (var x = 0; x < 5; x++) world.SetTile(x, 9, MaterialId.RedSandstone);
        var input = new InputManager(new InputBindings());
        var coyotePlayer = new PlayerController(new Vector2(4.25f * GameConstants.TileSize, 9f * GameConstants.TileSize));
        var leftGround = false;
        for (var frame = 0; frame < 40; frame++)
        {
            input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right);
            coyotePlayer.Update(input, world, GameConstants.FixedDelta);
            if (!coyotePlayer.Grounded) { leftGround = true; break; }
        }
        if (!leftGround) throw new InvalidOperationException("coyote test player did not leave the platform");
        input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right, InputAction.Jump);
        coyotePlayer.Update(input, world, GameConstants.FixedDelta);
        if (!coyotePlayer.UsedCoyoteThisFrame || coyotePlayer.Velocity.Y >= 0f)
            throw new InvalidOperationException("coyote jump was not consumed after leaving terrain");

        var bufferedPlayer = new PlayerController(new Vector2(3.5f * GameConstants.TileSize,
            9f * GameConstants.TileSize - 5f));
        bufferedPlayer.Velocity = new Vector2(0f, 120f);
        input = new InputManager(new InputBindings());
        input.SetSyntheticState(Vector2.Zero, Vector2.UnitX, InputAction.Jump);
        bufferedPlayer.Update(input, world, GameConstants.FixedDelta);
        var usedBuffer = false;
        for (var frame = 0; frame < 12; frame++)
        {
            input.SetSyntheticState(Vector2.Zero, Vector2.UnitX);
            bufferedPlayer.Update(input, world, GameConstants.FixedDelta);
            usedBuffer |= bufferedPlayer.UsedJumpBufferThisFrame;
            if (bufferedPlayer.Velocity.Y < 0f) break;
        }
        if (!usedBuffer || bufferedPlayer.Velocity.Y >= 0f)
            throw new InvalidOperationException("jump buffer did not fire on the first grounded frame");
    }

    private static void VerifyDashStopsAtTerrain()
    {
        var world = new TileWorld(16, 12);
        for (var x = 0; x < world.Width; x++) world.SetTile(x, 9, MaterialId.RedSandstone);
        for (var y = 0; y < 9; y++) world.SetTile(8, y, MaterialId.BlackBasalt);
        var player = new PlayerController(new Vector2(7.25f * GameConstants.TileSize, 9f * GameConstants.TileSize));
        var input = new InputManager(new InputBindings());
        input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right, InputAction.Dash);
        player.Update(input, world, GameConstants.FixedDelta);
        for (var frame = 0; frame < 8; frame++)
        {
            input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right);
            player.Update(input, world, GameConstants.FixedDelta);
        }
        if (player.State == MovementState.Dash || player.Bounds.Right > 8f * GameConstants.TileSize + 0.01f)
            throw new InvalidOperationException("dash survived or tunneled through a terrain collision");
    }

    private static void VerifyWallAndLedgeTransitions()
    {
        var world = new TileWorld(16, 14);
        for (var x = 0; x < world.Width; x++) world.SetTile(x, 12, MaterialId.RedSandstone);
        for (var x = 7; x <= 10; x++) world.SetTile(x, 8, MaterialId.RedSandstone);
        for (var y = 9; y < 12; y++) world.SetTile(7, y, MaterialId.RedSandstone);
        var input = new InputManager(new InputBindings());

        var wallPlayer = new PlayerController(new Vector2(7f * GameConstants.TileSize - PlayerController.BodyWidth * 0.5f,
            10.5f * GameConstants.TileSize));
        input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right, InputAction.Grab);
        wallPlayer.Update(input, world, GameConstants.FixedDelta);
        if (wallPlayer.State != MovementState.WallCling)
            throw new InvalidOperationException("one-tile player did not enter wall cling at a flush wall");
        input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right, InputAction.Grab, InputAction.Jump);
        wallPlayer.Update(input, world, GameConstants.FixedDelta);
        if (wallPlayer.Velocity.X >= 0f || wallPlayer.VisualState != MovementState.WallJump)
            throw new InvalidOperationException("wall jump did not leave the contacted wall with a readable state");

        var ledgePlayer = new PlayerController(new Vector2(7f * GameConstants.TileSize - PlayerController.BodyWidth * 0.5f,
            8f * GameConstants.TileSize + 11f));
        ledgePlayer.Velocity = new Vector2(0f, 75f);
        input = new InputManager(new InputBindings());
        input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right);
        ledgePlayer.Update(input, world, GameConstants.FixedDelta);
        if (ledgePlayer.State != MovementState.LedgeHang)
            throw new InvalidOperationException("one-tile player did not catch a clear ledge");
        for (var frame = 0; frame < 36 && ledgePlayer.State is MovementState.LedgeHang or MovementState.Mantle; frame++)
        {
            input.SetSyntheticState(new Vector2(1f, -1f), Vector2.UnitX, InputAction.Right, InputAction.Up);
            ledgePlayer.Update(input, world, GameConstants.FixedDelta);
        }
        if (!ledgePlayer.Grounded || ledgePlayer.Bounds.Center.X < 7f * GameConstants.TileSize)
            throw new InvalidOperationException($"mantle did not finish on top of the ledge: " +
                $"state={ledgePlayer.State} grounded={ledgePlayer.Grounded} position={ledgePlayer.Position} bounds={ledgePlayer.Bounds}");
    }

    private static void VerifyToolReachStopsAtAdjacentTile()
    {
        var world = new TileWorld(12, 10);
        for (var x = 0; x < world.Width; x++) world.SetTile(x, 7, MaterialId.RedSandstone);
        world.SetTile(5, 6, MaterialId.BlackBasalt);
        world.SetTile(6, 6, MaterialId.BlackBasalt);
        var player = new PlayerController(new Vector2(4.5f * GameConstants.TileSize, 7f * GameConstants.TileSize));
        var input = new InputManager(new InputBindings());
        var tool = new DigTool();
        input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right, InputAction.Dig);
        tool.Update(input, player, world, GameConstants.FixedDelta);
        for (var frame = 0; frame < 12; frame++)
        {
            input.SetSyntheticState(Vector2.UnitX, Vector2.UnitX, InputAction.Right);
            tool.Update(input, player, world, GameConstants.FixedDelta);
        }
        if (world.GetTile(5, 6).Damage != 1 || world.GetTile(6, 6).Damage != 0)
            throw new InvalidOperationException("tool swing did not stop at the adjacent terrain tile");
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

        VerifyBrittleTerrainReaction("bomb");
        VerifyBrittleTerrainReaction("burrower");

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

    private static void VerifyBrittleTerrainReaction(string cause)
    {
        var generated = ValidatedWorldGenerator.Generate(new WorldGenerationConfig());
        var brittle = new BrittleSystem(generated, DifficultyTuning.For(Difficulty.Easy));
        if (brittle.Structures.Count == 0) throw new InvalidOperationException("generation placed no brittle structure");
        var structure = brittle.Structures[0];
        var support = structure.Tiles[0];
        generated.Terrain.DamageTile(support.X, support.Y, 1, cause);
        if (!structure.Triggered)
            throw new InvalidOperationException($"{cause} terrain interaction did not trigger brittle structure");

        var player = new PlayerController(generated.Spawn);
        for (var frame = 0; frame < 60 && !structure.Collapsed; frame++)
            brittle.Update(player, generated.Terrain, GameConstants.FixedDelta);
        if (!structure.Collapsed)
            throw new InvalidOperationException($"brittle structure did not collapse after {cause} interaction");
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

    private static void VerifyImpactFeedback()
    {
        var camera = new Camera2D();
        camera.Snap(new Vector2(320f, 180f));
        camera.AddShake(10f);
        camera.Update(new Vector2(320f, 180f), Vector2.Zero, GameConstants.FixedDelta);
        if (camera.ShakeStrength <= 0f || camera.ShakeOffset.LengthSquared() <= 0f)
            throw new InvalidOperationException("impact did not produce camera shake");
        if (camera.VisibleWorldWidth <= GameConstants.VirtualWidth * 1.1f ||
            camera.VisibleWorldHeight <= GameConstants.VirtualHeight * 1.1f)
            throw new InvalidOperationException("camera does not expose the intended wider world framing");
        var worldPoint = new Vector2(402f, 217f);
        var projected = camera.WorldToScreen(worldPoint);
        if (Vector2.Distance(projected, Vector2.Transform(worldPoint, camera.View)) > 0.01f ||
            Vector2.Distance(worldPoint, camera.ScreenToWorld(projected)) > 0.01f)
            throw new InvalidOperationException("zoomed camera screen/world transforms disagree");

        var effects = new TerrainBreakEffects();
        effects.Emit(new TerrainChange(new Point(4, 5), MaterialId.RedSandstone, "tool", 1, true));
        if (effects.ActiveDebris < 12)
            throw new InvalidOperationException("block break did not produce a substantial debris burst");
    }

    private static void VerifyFallDamageTuning()
    {
        var world = new TileWorld(12, 36);
        for (var x = 0; x < world.Width; x++) world.SetTile(x, 32, MaterialId.RedSandstone);
        var input = new InputManager(new InputBindings());
        var player = new PlayerController(new Vector2(5.5f * GameConstants.TileSize, 2f * GameConstants.TileSize));
        for (var frame = 0; frame < 240 && !player.Grounded; frame++)
        {
            input.SetSyntheticState(Vector2.Zero, Vector2.UnitX);
            player.Update(input, world, GameConstants.FixedDelta);
        }
        if (!player.Grounded || player.Health != player.MaximumHealth - 1 || !player.Stunned)
            throw new InvalidOperationException("terminal fall should cost one heart and apply a readable stun");

        world = new TileWorld(12, 16);
        for (var x = 0; x < world.Width; x++) world.SetTile(x, 12, MaterialId.RedSandstone);
        player = new PlayerController(new Vector2(5.5f * GameConstants.TileSize,
            12f * GameConstants.TileSize - 42f));
        input = new InputManager(new InputBindings());
        for (var frame = 0; frame < 90 && !player.Grounded; frame++)
        {
            input.SetSyntheticState(Vector2.Zero, Vector2.UnitX);
            player.Update(input, world, GameConstants.FixedDelta);
        }
        if (!player.Grounded || player.Health != player.MaximumHealth)
            throw new InvalidOperationException("short recoverable fall caused damage");
    }
}
