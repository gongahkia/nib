using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Core;

namespace Summing.Rendering;

public sealed class AtmosphereRenderer
{
    private readonly Texture2D _pixel;
    public AtmosphereRenderer(Texture2D pixel) => _pixel = pixel;

    public void DrawWorldDither(SpriteBatch batch, Vector2 cameraPosition, long frame, float worldZoom)
    {
        var width = (int)MathF.Ceiling(GameConstants.VirtualWidth / worldZoom);
        var height = (int)MathF.Ceiling(GameConstants.VirtualHeight / worldZoom);
        var left = (int)cameraPosition.X - width / 2;
        var top = (int)cameraPosition.Y - height / 2;
        var drift = (int)(frame / 12 % 8);
        for (var y = top + 8; y < top + height; y += 12)
            for (var x = left + 8; x < left + width; x += 12)
            {
                if (((x / 12 + y / 12 + drift) & 3) != 0) continue;
                batch.Draw(_pixel, new Rectangle(x, y, 1, 1), new Color(143, 202, 196, 27));
            }
    }
}
