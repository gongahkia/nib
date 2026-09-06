using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Summing.Rendering;

public sealed class PixelFont
{
    private static readonly Dictionary<char, string[]> Glyphs = BuildGlyphs();
    private readonly Texture2D _pixel;
    public PixelFont(Texture2D pixel) => _pixel = pixel;

    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color, int scale = 1)
    {
        var originX = (int)position.X;
        var x = originX;
        var y = (int)position.Y;
        foreach (var raw in text.ToUpperInvariant())
        {
            if (raw == '\n') { x = originX; y += 8 * scale; continue; }
            var rows = Glyphs[Glyphs.ContainsKey(raw) ? raw : '?'];
            for (var row = 0; row < 7; row++)
            for (var column = 0; column < 5; column++)
            {
                if (rows[row][column] == '1')
                    batch.Draw(_pixel, new Rectangle(x + column * scale, y + row * scale, scale, scale), color);
            }
            x += 6 * scale;
        }
    }

    public Point Measure(string text, int scale = 1)
    {
        var lines = text.Split('\n');
        var width = 0;
        foreach (var line in lines) width = System.Math.Max(width, line.Length * 6 * scale);
        return new Point(width, lines.Length * 8 * scale);
    }

    private static Dictionary<char, string[]> BuildGlyphs()
    {
        var packed = new Dictionary<char, string>
        {
            ['A']="0111010001111111000110001", ['B']="1111010001111101000111110", ['C']="0111110000100001000001111",
            ['D']="1111010001100011000111110", ['E']="1111110000111101000011111", ['F']="1111110000111101000010000",
            ['G']="0111110000101111000101111", ['H']="1000110001111111000110001", ['I']="1111100100001000010011111",
            ['J']="0011100010000101001001100", ['K']="1000110010111001001010001", ['L']="1000010000100001000011111",
            ['M']="1000111011101011000110001", ['N']="1000111001101011001110001", ['O']="0111010001100011000101110",
            ['P']="1111010001111101000010000", ['Q']="0111010001101011001001101", ['R']="1111010001111101001010001",
            ['S']="0111110000011100000111110", ['T']="1111100100001000010000100", ['U']="1000110001100011000101110",
            ['V']="1000110001100010101000100", ['W']="1000110001101011101110001", ['X']="1000101010001000101010001",
            ['Y']="1000101010001000010000100", ['Z']="1111100010001000100011111", ['0']="0111010011101011100101110",
            ['1']="0010001100001000010001110", ['2']="0111010001000100010011111", ['3']="1111000001001100000111110",
            ['4']="0001000110010101111100010", ['5']="1111110000111100000111110", ['6']="0111010000111101000101110",
            ['7']="1111100010001000100001000", ['8']="0111010001011101000101110", ['9']="0111010001011110000101110",
            ['-']="0000000000111110000000000", [':']="0000000100000000010000000", ['.']="0000000000000000000000100",
            ['/']="0000100010001000100010000", ['[']="0110001000010000100001100", [']']="0011000010000100001000110",
            ['(']="0010001000010000100000100", [')']="0010000010000100010000100", ['+']="0000000100011100010000000",
            ['?']="0111010001001100000000100", [' ']="0000000000000000000000000", ['=']="0000011111000001111100000",
            ['%']="1100100010001000100010011", [',']="0000000000000000010001000", ['_']="0000000000000000000011111"
        };
        var result = new Dictionary<char, string[]>();
        foreach (var (key, bits) in packed)
        {
            var rows = new string[7];
            for (var row = 0; row < 7; row++) rows[row] = bits.Substring(row * 5, 5);
            result[key] = rows;
        }
        return result;
    }
}
