using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Summing.Entities;

public sealed class RopeEntity
{
    public RopeEntity(float x, float top, float bottom)
    {
        X = x;
        Top = top;
        Bottom = bottom;
    }

    public float X { get; }
    public float Top { get; }
    public float Bottom { get; }

    public void Draw(SpriteBatch batch, Texture2D pixel)
    {
        batch.Draw(pixel, new Rectangle((int)X - 1, (int)Top, 3, (int)(Bottom - Top)), new Color(160, 126, 85));
        for (var y = (int)Top + 7; y < Bottom; y += 12)
            batch.Draw(pixel, new Rectangle((int)X - 3, y, 7, 2), new Color(203, 165, 105));
    }
}
