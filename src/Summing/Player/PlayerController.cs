using System;
using Microsoft.Xna.Framework;
using Summing.Core;
using Summing.Input;
using Summing.World;

namespace Summing.Player;

public sealed class PlayerController
{
    public const float BodyWidth = 18f;
    public const float StandingBodyHeight = 22f;
    public const float CrouchingBodyHeight = 14f;
    private const float RunSpeed = 150f;
    private const float GroundAcceleration = 1300f;
    private const float AirAcceleration = 760f;
    private const float GroundFriction = 1500f;
    private const float Gravity = 1040f;
    private const float MaximumFallSpeed = 410f;
    private const float JumpSpeed = 330f;
    private const float WallJumpX = 230f;
    private const float DashSpeed = 390f;
    private const float CoyoteDuration = 0.10f;
    private const float JumpBufferDuration = 0.12f;
    private const float DashDuration = 0.13f;
    private const float FallDamageSpeed = 360f;
    private const float SevereFallSpeed = 405f;
    public const float WallStaminaMaximum = 2.25f;

    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _dashTimer;
    private float _stateTimer;
    private float _landingTimer;
    private float _ledgeTimer;
    private float _ledgeRegrabTimer;
    private int _wallDirection;
    private bool _jumpHeldLastFrame;
    private bool _shortBody;
    private MovementState _actionVisualState;
    private float _actionVisualTimer;
    private float _stunTimer;
    private Vector2 _mantleTarget;

    public PlayerController(Vector2 spawn, int maximumHealth = 5)
    {
        Position = spawn;
        WallStamina = WallStaminaMaximum;
        DashCharges = 1;
        Facing = 1;
        MaximumHealth = maximumHealth;
        Health = maximumHealth;
    }

    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; set; }
    public MovementState State { get; private set; } = MovementState.Falling;
    public MovementState VisualState => _actionVisualTimer > 0f ? _actionVisualState : State;
    public bool Grounded { get; private set; }
    public bool TouchingCeiling { get; private set; }
    public bool TouchingLeftWall { get; private set; }
    public bool TouchingRightWall { get; private set; }
    public bool LeftWallClimbable { get; private set; }
    public bool RightWallClimbable { get; private set; }
    public int Facing { get; private set; }
    public int DashCharges { get; private set; }
    public int MaximumDashCharges { get; set; } = 1;
    public float WallStamina { get; private set; }
    public bool UsedCoyoteThisFrame { get; private set; }
    public bool UsedJumpBufferThisFrame { get; private set; }
    public Aabb Bounds => BodyAt(Position, _shortBody);
    public int Health { get; private set; }
    public int MaximumHealth { get; }
    public bool Alive => Health > 0;
    public bool Stunned => _stunTimer > 0f;
    public event Action<string>? StatusEvent;
    public event Action? Dashed;

    public void Reset(Vector2 spawn)
    {
        Position = spawn;
        Velocity = Vector2.Zero;
        Grounded = false;
        TouchingCeiling = false;
        TouchingLeftWall = false;
        TouchingRightWall = false;
        LeftWallClimbable = false;
        RightWallClimbable = false;
        DashCharges = MaximumDashCharges;
        WallStamina = WallStaminaMaximum;
        State = MovementState.Falling;
        Health = MaximumHealth;
        _coyoteTimer = 0f;
        _jumpBufferTimer = 0f;
        _dashTimer = 0f;
        _stateTimer = 0f;
        _landingTimer = 0f;
        _ledgeTimer = 0f;
        _ledgeRegrabTimer = 0f;
        _wallDirection = 0;
        _jumpHeldLastFrame = false;
        _shortBody = false;
        _actionVisualTimer = 0f;
        _stunTimer = 0f;
    }

    public void Update(InputManager input, ICollisionWorld world, float dt)
    {
        UsedCoyoteThisFrame = false;
        UsedJumpBufferThisFrame = false;
        _actionVisualTimer = MathF.Max(0f, _actionVisualTimer - dt);
        if (!Alive) return;
        if (_stunTimer > 0f)
        {
            _stunTimer = MathF.Max(0f, _stunTimer - dt);
            Velocity = new Vector2(Velocity.X * 0.985f, MathF.Min(MaximumFallSpeed, Velocity.Y + Gravity * dt));
            MoveAndCollide(world, Velocity * dt);
            RefreshContacts(world);
            SetState(MovementState.Stunned);
            return;
        }
        _stateTimer += dt;
        _coyoteTimer = MathF.Max(0f, _coyoteTimer - dt);
        _jumpBufferTimer = MathF.Max(0f, _jumpBufferTimer - dt);
        _landingTimer = MathF.Max(0f, _landingTimer - dt);
        _ledgeRegrabTimer = MathF.Max(0f, _ledgeRegrabTimer - dt);
        RefreshContacts(world);

        if (Grounded)
        {
            _coyoteTimer = CoyoteDuration;
            DashCharges = MaximumDashCharges;
            WallStamina = WallStaminaMaximum;
        }
        if (input.Pressed(InputAction.Jump)) _jumpBufferTimer = JumpBufferDuration;

        if (State == MovementState.Mantle)
        {
            UpdateMantle(world, dt);
            return;
        }
        if (State == MovementState.LedgeHang)
        {
            UpdateLedge(input, dt);
            return;
        }
        if (input.Pressed(InputAction.Dash) && DashCharges > 0) BeginDash(input);

        if (_dashTimer > 0f)
        {
            _dashTimer -= dt;
            var collision = MoveAndCollide(world, Velocity * dt);
            RefreshContacts(world);
            if (collision.Horizontal || collision.Vertical)
            {
                _dashTimer = 0f;
                Velocity *= new Vector2(0.82f, 0.72f);
                ResolveState(0f, _shortBody);
            }
            else
            {
                if (_dashTimer <= 0f) Velocity *= new Vector2(0.82f, 0.72f);
                SetState(MovementState.Dash);
            }
            return;
        }

        var wantsCrouch = input.Down(InputAction.Down) || input.Down(InputAction.Slide);
        if (input.Pressed(InputAction.Slide) && Grounded && MathF.Abs(Velocity.X) > 55f)
        {
            Velocity = new Vector2(Facing * MathF.Max(235f, MathF.Abs(Velocity.X)), Velocity.Y);
            _shortBody = true;
            SetState(MovementState.Slide);
        }
        var shortBody = wantsCrouch || State == MovementState.Slide;
        if (!shortBody && _shortBody && world.OverlapsSolid(BodyAt(Position, false))) shortBody = true;
        _shortBody = shortBody;

        var moveX = input.Move.X;
        if (MathF.Abs(moveX) > 0.15f) Facing = Math.Sign(moveX);
        if (State == MovementState.Slide)
        {
            Velocity = new Vector2(Approach(Velocity.X, 0f, 480f * dt), Velocity.Y);
            if (MathF.Abs(Velocity.X) < 45f || !wantsCrouch) SetState(shortBody ? MovementState.Crouch : MovementState.Idle);
        }
        else
        {
            var target = shortBody ? moveX * 72f : moveX * RunSpeed;
            var acceleration = Grounded ? GroundAcceleration : AirAcceleration;
            if (MathF.Abs(moveX) < 0.12f && Grounded)
                acceleration = GroundFriction * Math.Clamp(world.FrictionAt(Position + new Vector2(0f, 2f)), 0.45f, 1.1f);
            Velocity = new Vector2(Approach(Velocity.X, target, acceleration * dt), Velocity.Y);
        }

        UpdateWallInteraction(input, dt);
        ConsumeBufferedJump(input);
        Velocity = new Vector2(Velocity.X, MathF.Min(MaximumFallSpeed, Velocity.Y + Gravity * dt));

        if (!input.Down(InputAction.Jump) && _jumpHeldLastFrame && Velocity.Y < -115f)
            Velocity = new Vector2(Velocity.X, Velocity.Y * 0.48f);
        _jumpHeldLastFrame = input.Down(InputAction.Jump);

        MoveAndCollide(world, Velocity * dt);
        RefreshContacts(world);
        TryCatchLedge(input, world);
        ResolveState(moveX, shortBody);
    }

    public void ShowActionState(MovementState state, float duration)
    {
        _actionVisualState = state;
        _actionVisualTimer = MathF.Max(_actionVisualTimer, duration);
    }

    public void ApplyDamage(int damage, Vector2 knockback, string cause)
    {
        if (!Alive || damage <= 0) return;
        Health = Math.Max(0, Health - damage);
        Velocity = knockback;
        _stunTimer = Health > 0 ? 0.48f : 0f;
        SetState(Health > 0 ? MovementState.Hurt : MovementState.Death);
        ShowActionState(State, Health > 0 ? 0.3f : 99f);
        StatusEvent?.Invoke(Health > 0 ? $"damage:{cause}:{damage}" : $"death:{cause}");
    }

    public void ApplyBlastImpulse(Vector2 impulse)
    {
        if (!Alive || impulse.LengthSquared() <= 0f) return;
        _dashTimer = 0f;
        _stunTimer = 0f;
        Velocity = new Vector2(
            Math.Clamp(Velocity.X + impulse.X, -480f, 480f),
            Math.Clamp(Velocity.Y + impulse.Y, -520f, MaximumFallSpeed));
        Grounded = false;
        SetState(Velocity.Y < -55f ? MovementState.AscendingJump : MovementState.Falling);
    }

    public bool TryRopeMove(Vector2 nextPosition, ICollisionWorld world)
    {
        if (!Alive) return false;
        var shortBody = world.OverlapsSolid(BodyAt(nextPosition, false));
        if ((shortBody && world.OverlapsSolid(BodyAt(nextPosition, true))) ||
            !RopePathClear(nextPosition, shortBody, world)) return false;
        Position = nextPosition;
        Velocity = Vector2.Zero;
        Grounded = false;
        _dashTimer = 0f;
        _shortBody = shortBody;
        SetState(MovementState.RopeInteraction);
        ShowActionState(MovementState.RopeInteraction, 0.08f);
        return true;
    }

    public bool TryRopeJump(int direction)
    {
        if (!Alive || Stunned) return false;
        direction = direction < 0 ? -1 : 1;
        Facing = direction;
        Velocity = new Vector2(direction * 205f, -310f);
        Grounded = false;
        _dashTimer = 0f;
        _coyoteTimer = 0f;
        _jumpBufferTimer = 0f;
        SetState(MovementState.RopeJump);
        ShowActionState(MovementState.RopeJump, 0.14f);
        return true;
    }

    private bool RopePathClear(Vector2 destination, bool shortBody, ICollisionWorld world)
    {
        var candidate = Position;
        if (destination.Y < candidate.Y)
        {
            if (!RopeAxisClear(ref candidate, destination.Y - candidate.Y, false, shortBody, world)) return false;
            return RopeAxisClear(ref candidate, destination.X - candidate.X, true, shortBody, world);
        }

        if (!RopeAxisClear(ref candidate, destination.X - candidate.X, true, shortBody, world)) return false;
        return RopeAxisClear(ref candidate, destination.Y - candidate.Y, false, shortBody, world);
    }

    private static bool RopeAxisClear(ref Vector2 candidate, float amount, bool horizontal, bool shortBody,
        ICollisionWorld world)
    {
        var remaining = amount;
        while (MathF.Abs(remaining) > 0.001f)
        {
            var step = Math.Clamp(remaining, -1f, 1f);
            candidate += horizontal ? new Vector2(step, 0f) : new Vector2(0f, step);
            if (world.OverlapsSolid(BodyAt(candidate, shortBody))) return false;
            remaining -= step;
        }
        return true;
    }

    public void Teleport(Vector2 position)
    {
        Position = position;
        Velocity = Vector2.Zero;
    }

    private static Aabb BodyAt(Vector2 footPosition, bool shortBody)
    {
        var height = shortBody ? CrouchingBodyHeight : StandingBodyHeight;
        return new Aabb(footPosition.X - BodyWidth * 0.5f, footPosition.Y - height, BodyWidth, height);
    }

    private void BeginDash(InputManager input)
    {
        var direction = input.Move.LengthSquared() >= 0.1f ? input.Move : input.Aim;
        if (direction.LengthSquared() < 0.1f) direction = new Vector2(Facing, 0f);
        direction.Normalize();
        DashCharges--;
        _dashTimer = DashDuration;
        Velocity = direction * DashSpeed;
        SetState(MovementState.Dash);
        Dashed?.Invoke();
    }

    private void UpdateWallInteraction(InputManager input, float dt)
    {
        var wall = TouchingLeftWall && LeftWallClimbable ? -1 : TouchingRightWall && RightWallClimbable ? 1 : 0;
        var wantsWall = wall != 0 && !Grounded && (input.Down(InputAction.Grab) || Math.Sign(input.Move.X) == wall);
        if (wantsWall && WallStamina > 0f && Velocity.Y >= -60f)
        {
            _wallDirection = wall;
            WallStamina = MathF.Max(0f, WallStamina - dt);
            var climb = -input.Move.Y * 72f;
            Velocity = new Vector2(Velocity.X, Approach(Velocity.Y, climb, 1150f * dt));
            SetState(MovementState.WallCling);
        }
        else if (State == MovementState.WallCling)
        {
            SetState(Velocity.Y < -55f ? MovementState.AscendingJump : MovementState.Falling);
        }
        if (wall != 0 && _jumpBufferTimer > 0f && !Grounded)
        {
            Velocity = new Vector2(-wall * WallJumpX, -JumpSpeed * 0.92f);
            Facing = -wall;
            _jumpBufferTimer = 0f;
            SetState(MovementState.WallJump);
            ShowActionState(MovementState.WallJump, 0.12f);
        }
    }

    private void ConsumeBufferedJump(InputManager input)
    {
        if (_jumpBufferTimer <= 0f || _coyoteTimer <= 0f) return;
        UsedCoyoteThisFrame = !Grounded;
        UsedJumpBufferThisFrame = Grounded && !input.Pressed(InputAction.Jump);
        Velocity = new Vector2(Velocity.X, -JumpSpeed);
        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;
        SetState(MovementState.Takeoff);
        ShowActionState(MovementState.Takeoff, 0.08f);
    }

    private void TryCatchLedge(InputManager input, ICollisionWorld world)
    {
        if (Grounded || Velocity.Y < 0f || State == MovementState.Dash || _ledgeRegrabTimer > 0f ||
            input.Down(InputAction.Down)) return;
        var direction = MathF.Abs(input.Move.X) > 0.2f ? Math.Sign(input.Move.X) : Facing;
        if (input.Move.X * direction < -0.2f) return;
        var body = Bounds;
        var chestProbe = new Aabb(direction > 0 ? body.Right : body.Left - 2f, body.Top + 6f, 2f, 13f);
        var headProbe = new Aabb(chestProbe.X, body.Top - 7f, 2f, 10f);
        if (!world.OverlapsSolid(chestProbe) || world.OverlapsSolid(headProbe) ||
            !TryFindMantleTarget(world, direction, out _mantleTarget)) return;
        _wallDirection = direction;
        Velocity = Vector2.Zero;
        _ledgeTimer = 0f;
        SetState(MovementState.LedgeHang);
    }

    private void UpdateLedge(InputManager input, float dt)
    {
        _ledgeTimer += dt;
        Velocity = Vector2.Zero;
        if (input.Down(InputAction.Down) || input.Move.X * _wallDirection < -0.3f)
        {
            Position += new Vector2(-_wallDirection * 3f, 3f);
            _ledgeRegrabTimer = 0.16f;
            SetState(MovementState.Falling);
            return;
        }
        if (input.Pressed(InputAction.Jump) || input.Down(InputAction.Up) || _ledgeTimer > 0.32f) SetState(MovementState.Mantle);
    }

    private void UpdateMantle(ICollisionWorld world, float dt)
    {
        var toTarget = _mantleTarget - Position;
        var verticalStep = Math.Clamp(toTarget.Y, -185f * dt, 185f * dt);
        if (MathF.Abs(toTarget.Y) > 0.5f) MoveAndCollide(world, new Vector2(0f, verticalStep));
        toTarget = _mantleTarget - Position;
        if (MathF.Abs(toTarget.Y) <= 2f)
        {
            var horizontalStep = Math.Clamp(toTarget.X, -220f * dt, 220f * dt);
            MoveAndCollide(world, new Vector2(horizontalStep, 0f));
        }
        RefreshContacts(world);
        if (Vector2.DistanceSquared(Position, _mantleTarget) <= 2.25f || _stateTimer >= 0.48f)
        {
            Velocity = Vector2.Zero;
            SetState(Grounded ? MovementState.Idle : MovementState.Falling);
        }
    }

    private CollisionResult MoveAndCollide(ICollisionWorld world, Vector2 delta)
    {
        var wasGrounded = Grounded;
        var hitX = MoveAxis(world, delta.X, true);
        var hitY = MoveAxis(world, delta.Y, false);
        if (hitX) Velocity = new Vector2(0f, Velocity.Y);
        if (hitY)
        {
            if (delta.Y > 0f && !wasGrounded) _landingTimer = 0.09f;
            if (delta.Y > 0f) HandleLanding(Velocity.Y);
            Velocity = new Vector2(Velocity.X, 0f);
        }
        return new CollisionResult(hitX, hitY);
    }

    private void HandleLanding(float fallSpeed)
    {
        if (fallSpeed < FallDamageSpeed) return;
        ApplyDamage(1, new Vector2(Velocity.X * 0.3f, -90f), "fall");
        _stunTimer = fallSpeed >= SevereFallSpeed ? 0.7f : 0.4f;
        StatusEvent?.Invoke($"fall:{(int)fallSpeed}");
    }

    private bool MoveAxis(ICollisionWorld world, float amount, bool horizontal)
    {
        var remaining = amount;
        while (MathF.Abs(remaining) > 0.0001f)
        {
            var step = Math.Clamp(remaining, -1f, 1f);
            var candidate = Position + (horizontal ? new Vector2(step, 0f) : new Vector2(0f, step));
            if (world.OverlapsSolid(BodyAt(candidate, _shortBody))) return true;
            Position = candidate;
            remaining -= step;
        }
        return false;
    }

    private void RefreshContacts(ICollisionWorld world)
    {
        var body = Bounds;
        Grounded = world.OverlapsSolid(body.Offset(0f, 1f));
        TouchingCeiling = world.OverlapsSolid(body.Offset(0f, -1f));
        TouchingLeftWall = world.OverlapsSolid(body.Offset(-1f, 0f));
        TouchingRightWall = world.OverlapsSolid(body.Offset(1f, 0f));
        LeftWallClimbable = TouchingLeftWall && world.IsClimbable(new Vector2(body.Left - 1f, body.Center.Y));
        RightWallClimbable = TouchingRightWall && world.IsClimbable(new Vector2(body.Right + 1f, body.Center.Y));
    }

    private void ResolveState(float moveX, bool shortBody)
    {
        if (_dashTimer > 0f || State is MovementState.LedgeHang or MovementState.Mantle) return;
        if (State == MovementState.Slide && Grounded && shortBody && MathF.Abs(Velocity.X) >= 45f) return;
        if (State == MovementState.WallCling && !Grounded && (TouchingLeftWall || TouchingRightWall)) return;
        if (Grounded)
        {
            if (_landingTimer > 0f) SetState(MovementState.Landing);
            else if (shortBody) SetState(MathF.Abs(moveX) > 0.15f ? MovementState.Crawl : MovementState.Crouch);
            else SetState(MathF.Abs(Velocity.X) > 15f ? MovementState.Run : MovementState.Idle);
        }
        else if (Velocity.Y < -55f) SetState(MovementState.AscendingJump);
        else if (MathF.Abs(Velocity.Y) <= 55f) SetState(MovementState.Apex);
        else SetState(MovementState.Falling);
    }

    private void SetState(MovementState state)
    {
        if (State == state) return;
        State = state;
        _stateTimer = 0f;
    }

    private static float Approach(float value, float target, float amount) =>
        value < target ? MathF.Min(value + amount, target) : MathF.Max(value - amount, target);

    private bool TryFindMantleTarget(ICollisionWorld world, int direction, out Vector2 target)
    {
        for (var rise = 4f; rise <= GameConstants.TileSize + 10f; rise += 1f)
            for (var across = 10f; across <= GameConstants.TileSize + BodyWidth; across += 1f)
            {
                var candidate = Position + new Vector2(direction * across, -rise);
                var body = BodyAt(candidate, false);
                if (world.OverlapsSolid(body) || !world.OverlapsSolid(body.Offset(0f, 1f))) continue;
                target = candidate;
                return true;
            }
        target = Position;
        return false;
    }

    private readonly record struct CollisionResult(bool Horizontal, bool Vertical);

}
