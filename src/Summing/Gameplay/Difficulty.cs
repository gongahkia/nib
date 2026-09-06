namespace Summing.Gameplay;

public enum Difficulty
{
    Easy,
    Hard
}

public sealed record DifficultyTuning(
    int StartingHealth,
    int StartingBombs,
    int StartingRopes,
    int EnemyContactDamage,
    float HazardDelayMultiplier,
    float EnemySpeedMultiplier)
{
    public static DifficultyTuning For(Difficulty difficulty) => difficulty == Difficulty.Hard
        ? new DifficultyTuning(3, 2, 2, 2, 0.7f, 1.2f)
        : new DifficultyTuning(5, 4, 4, 1, 1.15f, 0.88f);
}
