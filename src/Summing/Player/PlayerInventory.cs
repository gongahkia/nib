using Summing.Gameplay;

namespace Summing.Player;

public sealed class PlayerInventory
{
    public PlayerInventory(DifficultyTuning tuning)
    {
        Bombs = tuning.StartingBombs;
        Ropes = tuning.StartingRopes;
    }

    public int Bombs { get; private set; }
    public int Ropes { get; private set; }
    public bool TryUseBomb() { if (Bombs <= 0) return false; Bombs--; return true; }
    public bool TryUseRope() { if (Ropes <= 0) return false; Ropes--; return true; }
}
