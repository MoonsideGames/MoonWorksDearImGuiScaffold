using MoonWorks;
using MoonWorks.Graphics;

namespace MoonWorksDearImGuiScaffold;

class Program
{
	static void Main(string[] args)
	{
		WindowCreateInfo windowCreateInfo = new WindowCreateInfo
		{
			WindowWidth = 1280,
			WindowHeight = 720,
			WindowTitle = "MoonWorksDearImGuiScaffold",
			ScreenMode = ScreenMode.Windowed,
			PresentMode = PresentMode.FIFORelaxed,
			SystemResizable = true
		};

		FrameLimiterSettings frameLimiterSettings = new FrameLimiterSettings
		{
			Mode = FrameLimiterMode.Capped,
			Cap = 60
		};

		var debugMode = false;

		#if DEBUG
		debugMode = true;
		#endif

		MoonWorksDearImGuiScaffoldGame game = new MoonWorksDearImGuiScaffoldGame(
			windowCreateInfo,
			frameLimiterSettings,
			[Backend.Vulkan, Backend.D3D11],
			debugMode
		);

		game.Run();
	}
}
