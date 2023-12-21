using System;
using System.Collections.Generic;
using MoonWorks.Graphics;

namespace MoonWorksDearImGuiScaffold;

public class TextureStorage
{
    Dictionary<IntPtr, WeakReference<Texture>> PointerToTexture = new Dictionary<IntPtr, WeakReference<Texture>>();

    public IntPtr Add(Texture texture)
    {
        if (!PointerToTexture.ContainsKey(texture.Handle))
        {
            PointerToTexture.Add(texture.Handle, new WeakReference<Texture>(texture));
        }
        return texture.Handle;
    }

    public Texture GetTexture(IntPtr pointer)
    {
        if (!PointerToTexture.ContainsKey(pointer))
        {
            return null;
        }

        var result = PointerToTexture[pointer];

        if (!result.TryGetTarget(out var texture))
        {
            PointerToTexture.Remove(pointer);
            return null;
        }

        return texture;
    }
}
