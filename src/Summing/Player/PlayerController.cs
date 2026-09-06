using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Core;
using Summing.Input;
using Summing.World;

namespace Summing.Player;

public sealed class PlayerController
{
    private const float StandingWidth = 18f;
    private const float StandingHeight = 42f;
    private const float CrouchingHeight = 21f;
    private const float RunSpeed = 150f;
    private const float GroundAcceleration = 1300f;
    private const float AirAcceleration = 760f;
    private const float GroundFriction = 1500f;
    private const float Gravity = 1040f;
    private const float MaximumFallSpeed = 410f;
    private const float JumpSpeed = 330f;
    private const float WallJumpX = 230f;
    private const float DashSpeed = 390f;
    private const float GrappleSpeed = 275f;
    private const float GrappleRange = 250f;
    private const float CoyoteDuration = 0.10f;
    private const float JumpBufferDuration = 0.12f;
    private const float DashDuration = 0.13f;
    public const float WallStaminaMaximum = 2.25f;

    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _dashTimer;
    private float _stateTimer;
    private float _landingTimer;
    private float _ledgeTimer;
    private int _wallDirection;
    private bool _jumpHeldLastFrame;
    private bool _shortBody;
    private MovementState _actionVisualState;
    private float _actionVisualTimer;
    private float _stunTimer;

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
    public bool TouchingLeftWall { get; private set; }
    public bool TouchingRightWall { get; private set; }
    public int Facing { get; private set; }
    public int DashCharges { get; private set; }
    public int MaximumDashCharges { get; set; } = 1;
    public float WallStamina { get; private set; }
    public bool GrappleAttached { get; private set; }
    public Vector2 GrappleAnchor { get; private set; }
    public bool UsedCoyoteThisFrame { get; private set; }
    public bool UsedJumpBufferThisFrame { get; private set; }
    public Aabb Bounds => BodyAt(Position, _shortBody);
    public int Health { get; private set; }
    public int MaximumHealth { get; }
    public bool Alive => Health > 0;
    public bool Stunned => _stunTimer > 0f;
    public event Action<string>? StatusEvent;

    public void Reset(Vector2 spawn)
    {
        Position = spawn;
        Velocity = Vector2.Zero;
        Grounded = false;
        GrappleAttached = false;
        DashCharges = MaximumDashCharges;
        WallStamina = WallStaminaMaximum;
        State = MovementState.Falling;
        Health = MaximumHealth;
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
            UpdateLedge(input);
            return;
        }
        if (input.Pressed(InputAction.Dash) && DashCharges > 0) BeginDash(input);
        UpdateGrapple(input, world);

        if (_dashTimer > 0f)
        {
            _dashTimer -= dt;
            MoveAndCollide(world, Velocity * dt);
            if (_dashTimer <= 0f) Velocity *= new Vector2(0.82f, 0.72f);
            SetState(MovementState.Dash);
            RefreshContacts(world);
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
            if (MathF.Abs(moveX) < 0.12f && Grounded) acceleration = GroundFriction;
            Velocity = new Vector2(Approach(Velocity.X, target, acceleration * dt), Velocity.Y);
        }

        UpdateWallInteraction(input, dt);
        ConsumeBufferedJump(input);
        if (!GrappleAttached)
            Velocity = new Vector2(Velocity.X, MathF.Min(MaximumFallSpeed, Velocity.Y + Gravity * dt));
        else
            PullTowardGrapple(dt);

        if (!input.Down(InputAction.Jump) && _jumpHeldLastFrame && Velocity.Y < -115f)
            Velocity = new Vector2(Velocity.X, Velocity.Y * 0.48f);
        _jumpHeldLastFrame = input.Down(InputAction.Jump);

        MoveAndCollide(world, Velocity * dt);
        RefreshContacts(world);
        TryCatchLedge(input, world);
        ResolveState(moveX, shortBody);
    }

    public void Draw(SpriteBatch batch, Texture2D pixel)
    {
        var bounds = Bounds;
        var body = new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
        var color = VisualState switch
        {
            MovementState.Dash => new Color(151, 224, 219),
            MovementState.Grapple => new Color(220, 179, 90),
            MovementState.WallCling => new Color(180, 137, 111),
            MovementState.Slide => new Color(228, 97, 75),
            MovementState.LedgeHang or MovementState.Mantle => new Color(216, 203, 154),
            _ => new Color(206, 185, 112)
        };
        batch.Draw(pixel, body, color);
        batch.Draw(pixel, new Rectangle(body.X + (Facing > 0 ? body.Width - 5 : 1), body.Y + 7, 4, 4), new Color(18, 20, 27));
        batch.Draw(pixel, new Rectangle(body.X + 3, body.Bottom - 5, body.Width - 6, 3), new Color(109, 50, 52));
        if (GrappleAttached)
        {
            DrawLine(batch, pixel, Bounds.Center, GrappleAnchor, new Color(184, 166, 128), 2f);
            batch.Draw(pixel, new Rectangle((int)GrappleAnchor.X - 2, (int)GrappleAnchor.Y - 2, 5, 5), Color.White);
        }
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
        GrappleAttached = false;
        _stunTimer = Health > 0 ? 0.48f : 0f;
        SetState(Health > 0 ? MovementState.Hurt : MovementState.Death);
        ShowActionState(State, Health > 0 ? 0.3f : 99f);
        StatusEvent?.Invoke(Health > 0 ? $"damage:{cause}:{damage}" : $"death:{cause}");
    }

    public bool TryRopeMove(Vector2 nextPosition, ICollisionWorld world)
    {
        if (!Alive || world.OverlapsSolid(BodyAt(nextPosition, _shortBody))) return false;
        Position = nextPosition;
        Velocity = Vector2.Zero;
        ShowActionState(MovementState.RopeInteraction, 0.08f);
        return true;
    }

    public void Teleport(Vector2 position)
    {
        Position = position;
        Velocity = Vector2.Zero;
        GrappleAttached = false;
    }

    private static Aabb BodyAt(Vector2 footPosition, bool shortBody)
    {
        var height = shortBody ? CrouchingHeight : StandingHeight;
        return new Aabb(footPosition.X - StandingWidth * 0.5f, footPosition.Y - height, StandingWidth, height);
    }

    private void BeginDash(InputManager input)
    {
        var direction = input.Move.LengthSquared() >= 0.1f ? input.Move : input.Aim;
        if (direction.LengthSquared() < 0.1f) direction = new Vector2(Facing, 0f);
        direction.Normalize();
        DashCharges--;
        _dashTimer = DashDuration;
        Velocity = direction * DashSpeed;
        GrappleAttached = false;
        SetState(MovementState.Dash);
    }

    private void UpdateGrapple(InputManager input, ICollisionWorld world)
    {
        if (input.Pressed(InputAction.Grapple) && world.RaycastGrapple(Bounds.Center, input.Aim, GrappleRange, out var anchor))
        {
            GrappleAttached = true;
            GrappleAnchor = anchor;
            SetState(MovementState.Grapple);
        }
        if (input.Released(InputAction.Grapple)) GrappleAttached = false;
    }

    private void PullTowardGrapple(float dt)
    {
        var toAnchor = GrappleAnchor - Bounds.Center;
        if (toAnchor.LengthSquared() < 12f * 12f) { Velocity *= 0.85f; return; }
        var direction = Vector2.Normalize(toAnchor);
        var tangential = Velocity - direction * Vector2.Dot(Velocity, direction);
        Velocity = tangential * 0.94f + direction * GrappleSpeed;
        Velocity += new Vector2(0f, Gravity * 0.18f * dt);
        SetState(MovementState.Grapple);
    }

    private void UpdateWallInteraction(InputManager input, float dt)
    {
        var wall = TouchingLeftWall ? -1 : TouchingRightWall ? 1 : 0;
        var wantsWall = wall != 0 && !Grounded && (input.Down(InputAction.Grab) || Math.Sign(input.Move.X) == wall);
        if (wantsWall && WallStamina > 0f && Velocity.Y >= -60f)
        {
            _wallDirection = wall;
            WallStamina = MathF.Max(0f, WallStamina - dt);
            var climb = -input.Move.Y * 72f;
            Velocity = new Vector2(Velocity.X, Approach(Velocity.Y, climb, 1150f * dt));
            SetState(MovementState.WallCling);
        }
        if (wall != 0 && _jumpBufferTimer > 0f && !Grounded)
        {
            Velocity = new Vector2(-wall * WallJumpX, -JumpSpeed * 0.92f);
            Facing = -wall;
            _jumpBufferTimer = 0f;
            GrappleAttached = false;
            SetState(MovementState.WallJump);
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
        GrappleAttached = false;
        SetState(MovementState.Takeoff);
    }

    private void TryCatchLedge(InputManager input, ICollisionWorld world)
    {
        if (Grounded || Velocity.Y < 0f || State == MovementState.Dash) return;
        var direction = MathF.Abs(input.Move.X) > 0.2f ? Math.Sign(input.Move.X) : Facing;
        var body = Bounds;
        var chestProbe = new Aabb(direction > 0 ? body.Right : body.Left - 2f, body.Top + 15f, 2f, 16f);
        var headProbe = new Aabb(chestProbe.X, body.Top - 5f, 2f, 12f);
        var clearance = BodyAt(new Vector2(Position.X, Position.Y - 18f), false);
        if (!world.OverlapsSolid(chestProbe) || world.OverlapsSolid(headProbe) || world.OverlapsSolid(clearance)) return;
        _wallDirection = direction;
        Velocity = Vector2.Zero;
        _ledgeTimer = 0f;
        SetState(MovementState.LedgeHang);
    }

    private void UpdateLedge(InputManager input)
    {
        _ledgeTimer += GameConstants.FixedDelta;
        Velocity = Vector2.Zero;
        if (input.Down(InputAction.Down) || input.Move.X * _wallDirection < -0.3f)
        {
            Position += new Vector2(-_wallDirection * 3f, 3f);
            SetState(MovementState.Falling);
            return;
        }
        if (input.Pressed(InputAction.Jump) || input.Down(InputAction.Up) || _ledgeTimer > 0.32f) SetState(MovementState.Mantle);
    }

    private void UpdateMantle(ICollisionWorld world, float dt)
    {
        MoveAndCollide(world, new Vector2(_wallDirection * 70f, -105f) * dt);
        if (_stateTimer < 0.22f) return;
        Velocity = new Vector2(_wallDirection * 70f, 0f);
        SetState(MovementState.Idle);
    }

    private void MoveAndCollide(ICollisionWorld world, Vector2 delta)
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
    }

    private void HandleLanding(float fallSpeed)
    {
        if (fallSpeed < 335f) return;
        var damage = fallSpeed >= 395f ? 2 : 1;
        ApplyDamage(damage, new Vector2(Velocity.X * 0.3f, -105f), "fall");
        _stunTimer = fallSpeed >= 395f ? 0.9f : 0.55f;
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
        TouchingLeftWall = world.OverlapsSolid(body.Offset(-1f, 0f));
        TouchingRightWall = world.OverlapsSolid(body.Offset(1f, 0f));
    }

    private void ResolveState(float moveX, bool shortBody)
    {
        if (_dashTimer > 0f || GrappleAttached || State is MovementState.LedgeHang or MovementState.Mantle) return;
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

    private static void DrawLine(SpriteBatch batch, Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
    {
        var delta = end - start;
        batch.Draw(pixel, start, null, color, MathF.Atan2(delta.Y, delta.X), Vector2.Zero,
            new Vector2(delta.Length(), width), SpriteEffects.None, 0f);
    }
}
