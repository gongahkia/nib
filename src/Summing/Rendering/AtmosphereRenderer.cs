using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Core;

namespace Summing.Rendering;

public sealed class AtmosphereRenderer
{
    private readonly Texture2D _pixel;
    public AtmosphereRenderer(Texture2D pixel) => _pixel = pixel;

    public void DrawWorldDither(SpriteBatch batch, Vector2 cameraPosition, long frame)
    {
        var left = (int)cameraPosition.X - GameConstants.VirtualWidth / 2;
        var top = (int)cameraPosition.Y - GameConstants.VirtualHeight / 2;
        var drift = (int)(frame / 12 % 8);
        for (var y = top + 8; y < top + GameConstants.VirtualHeight; y += 12)
        for (var x = left + 8; x < left + GameConstants.VirtualWidth; x += 12)
        {
            if (((x / 12 + y / 12 + drift) & 3) != 0) continue;
            batch.Draw(_pixel, new Rectangle(x, y, 1, 1), new Color(143, 202, 196, 27));
        }
    }
}
