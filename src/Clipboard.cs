using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MoonWorksDearImGuiScaffold;

public static unsafe class Clipboard
{
	private static IntPtr clipboard;
	private static readonly Dictionary<Delegate, IntPtr> pinned = new();

	private static unsafe void Set(void* userdata, byte* text)
	{
		var len = 0; while (text[len] != 0) len++;
		var str = Encoding.UTF8.GetString(text, len);
		SDL2.SDL.SDL_SetClipboardText(str);
	}

	private static unsafe byte* Get(void* userdata)
	{
		if (clipboard != IntPtr.Zero)
		{
			NativeMemory.Free((void*) clipboard);
			clipboard = IntPtr.Zero;
		}

		var str = SDL2.SDL.SDL_GetClipboardText();
		var length = Encoding.UTF8.GetByteCount(str);
		var bytes = (byte*)(clipboard = (nint)NativeMemory.Alloc((nuint)(length + 1)));

		Encoding.UTF8.GetBytes(str, new Span<byte>(bytes, length));
		bytes[length] = 0;
		return bytes;
	}

	// Stops the delegate pointer from being collected
	private static IntPtr GetPointerTo<T>(T fn) where T : Delegate
	{
		if (pinned.TryGetValue(fn, out var ptr))
			return ptr;

		ptr = Marshal.GetFunctionPointerForDelegate(fn);
		pinned.Add(fn, ptr);
		return ptr;
	}

	public static readonly IntPtr GetFnPtr = GetPointerTo(Get);
	public static readonly IntPtr SetFnPtr = GetPointerTo(Set);
}
