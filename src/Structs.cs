using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace MoonWorksDearImGuiScaffold;

public struct Position2DTextureColorVertex : IVertexType
{
    public Vector2 Position;
    public Vector2 TexCoord;
    public Color Color;

    public Position2DTextureColorVertex(
        Vector2 position,
        Vector2 texcoord,
        Color color
    )
    {
        Position = position;
        TexCoord = texcoord;
        Color = color;
    }

    public static VertexElementFormat[] Formats { get; } = new VertexElementFormat[3]
    {
        VertexElementFormat.Vector2,
        VertexElementFormat.Vector2,
        VertexElementFormat.Color
    };
}
