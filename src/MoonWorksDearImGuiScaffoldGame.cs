using MoonWorks.Graphics;
using MoonWorks.Input;
using MoonWorks;
using ImGuiNET;
using System.IO;
using MoonWorks.Math.Float;
using System;

namespace MoonWorksDearImGuiScaffold;

class MoonWorksDearImGuiScaffoldGame : Game
{
	private string ShaderContentPath = Path.Combine(System.AppContext.BaseDirectory, "Content", "Shaders");

	private GraphicsPipeline ImGuiPipeline;
	private ShaderModule ImGuiVertexShader;
	private ShaderModule ImGuiFragmentShader;
	private Sampler ImGuiSampler;

	private uint VertexCount = 0;
	private uint IndexCount = 0;
	private MoonWorks.Graphics.Buffer ImGuiVertexBuffer = null;
	private MoonWorks.Graphics.Buffer ImGuiIndexBuffer = null;

	private TextureStorage TextureStorage;

	public MoonWorksDearImGuiScaffoldGame(
		WindowCreateInfo windowCreateInfo,
		FrameLimiterSettings frameLimiterSettings,
		bool debugMode
	) : base(windowCreateInfo, frameLimiterSettings, 60, debugMode)
	{
		TextureStorage = new TextureStorage();

		ImGui.CreateContext();

		var io = ImGui.GetIO();
		io.DisplaySize = new System.Numerics.Vector2(MainWindow.Width, MainWindow.Height);
		io.DisplayFramebufferScale = System.Numerics.Vector2.One;

		ImGuiVertexShader =
			new ShaderModule(GraphicsDevice, Path.Combine(ShaderContentPath, "ImGui.vert.refresh"));
		ImGuiFragmentShader =
			new ShaderModule(GraphicsDevice, Path.Combine(ShaderContentPath, "ImGui.frag.refresh"));


		ImGuiSampler = new Sampler(GraphicsDevice, SamplerCreateInfo.LinearClamp);

		ImGuiPipeline = new GraphicsPipeline(
			GraphicsDevice,
			new GraphicsPipelineCreateInfo
			{
				AttachmentInfo = new GraphicsPipelineAttachmentInfo(
					new ColorAttachmentDescription(
						MainWindow.SwapchainFormat,
						ColorAttachmentBlendState.NonPremultiplied
					)
				),
				DepthStencilState = DepthStencilState.Disable,
				VertexShaderInfo = GraphicsShaderInfo.Create<Matrix4x4>(ImGuiVertexShader, "main", 0),
				FragmentShaderInfo = GraphicsShaderInfo.Create(ImGuiFragmentShader, "main", 1),
				VertexInputState = VertexInputState.CreateSingleBinding<Position2DTextureColorVertex>(),
				PrimitiveType = PrimitiveType.TriangleList,
				RasterizerState = RasterizerState.CW_CullNone,
				MultisampleState = MultisampleState.None
			}
		);

		BuildFontAtlas();

		MainWindow.RegisterSizeChangeCallback(HandleSizeChange);

		Inputs.TextInput += c =>
		{
			if (c == '\t') { return; }
			io.AddInputCharacter(c);
		};

		io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

		if (!OperatingSystem.IsWindows())
		{
			io.SetClipboardTextFn = Clipboard.SetFnPtr;
			io.GetClipboardTextFn = Clipboard.GetFnPtr;
		}
	}

	protected void HandleSizeChange(uint width, uint height)
	{
		var io = ImGui.GetIO();
		io.DisplaySize = new System.Numerics.Vector2(width, height);
	}

	protected override void Update(System.TimeSpan dt)
	{
		var io = ImGui.GetIO();

		io.MousePos = new System.Numerics.Vector2(Inputs.Mouse.X, Inputs.Mouse.Y);
		io.MouseDown[0] = Inputs.Mouse.LeftButton.IsDown;
		io.MouseDown[1] = Inputs.Mouse.RightButton.IsDown;
		io.MouseDown[2] = Inputs.Mouse.MiddleButton.IsDown;
		io.MouseWheel = Inputs.Mouse.Wheel;

		io.AddKeyEvent(ImGuiKey.A, Inputs.Keyboard.IsDown(KeyCode.A));
		io.AddKeyEvent(ImGuiKey.Z, Inputs.Keyboard.IsDown(KeyCode.Z));
		io.AddKeyEvent(ImGuiKey.Y, Inputs.Keyboard.IsDown(KeyCode.Y));
		io.AddKeyEvent(ImGuiKey.X, Inputs.Keyboard.IsDown(KeyCode.X));
		io.AddKeyEvent(ImGuiKey.C, Inputs.Keyboard.IsDown(KeyCode.C));
		io.AddKeyEvent(ImGuiKey.V, Inputs.Keyboard.IsDown(KeyCode.V));

		io.AddKeyEvent(ImGuiKey.Tab, Inputs.Keyboard.IsDown(KeyCode.Tab));
		io.AddKeyEvent(ImGuiKey.LeftArrow, Inputs.Keyboard.IsDown(KeyCode.Left));
		io.AddKeyEvent(ImGuiKey.RightArrow, Inputs.Keyboard.IsDown(KeyCode.Right));
		io.AddKeyEvent(ImGuiKey.UpArrow, Inputs.Keyboard.IsDown(KeyCode.Up));
		io.AddKeyEvent(ImGuiKey.DownArrow, Inputs.Keyboard.IsDown(KeyCode.Down));
		io.AddKeyEvent(ImGuiKey.Enter, Inputs.Keyboard.IsDown(KeyCode.Return));
		io.AddKeyEvent(ImGuiKey.Escape, Inputs.Keyboard.IsDown(KeyCode.Escape));
		io.AddKeyEvent(ImGuiKey.Delete, Inputs.Keyboard.IsDown(KeyCode.Delete));
		io.AddKeyEvent(ImGuiKey.Backspace, Inputs.Keyboard.IsDown(KeyCode.Backspace));
		io.AddKeyEvent(ImGuiKey.Home, Inputs.Keyboard.IsDown(KeyCode.Home));
		io.AddKeyEvent(ImGuiKey.End, Inputs.Keyboard.IsDown(KeyCode.End));
		io.AddKeyEvent(ImGuiKey.PageDown, Inputs.Keyboard.IsDown(KeyCode.PageDown));
		io.AddKeyEvent(ImGuiKey.PageUp, Inputs.Keyboard.IsDown(KeyCode.PageUp));
		io.AddKeyEvent(ImGuiKey.Insert, Inputs.Keyboard.IsDown(KeyCode.Insert));

		io.AddKeyEvent(ImGuiKey.ModCtrl, Inputs.Keyboard.IsDown(KeyCode.LeftControl) || Inputs.Keyboard.IsDown(KeyCode.RightControl));
		io.AddKeyEvent(ImGuiKey.ModShift, Inputs.Keyboard.IsDown(KeyCode.LeftShift) || Inputs.Keyboard.IsDown(KeyCode.RightShift));
		io.AddKeyEvent(ImGuiKey.ModAlt, Inputs.Keyboard.IsDown(KeyCode.LeftAlt) || Inputs.Keyboard.IsDown(KeyCode.RightAlt));
		io.AddKeyEvent(ImGuiKey.ModSuper, Inputs.Keyboard.IsDown(KeyCode.LeftMeta) || Inputs.Keyboard.IsDown(KeyCode.RightMeta));

		ImGui.NewFrame();

		ImGuiCommands();

		ImGui.EndFrame();
	}

	protected void ImGuiCommands()
	{
		// Do your ImGui commands here!
		ImGui.ShowDemoWindow();
	}

	protected override void Draw(double alpha)
	{
		ImGui.Render();

		var io = ImGui.GetIO();
		var drawDataPtr = ImGui.GetDrawData();

		UpdateImGuiBuffers(drawDataPtr);

		var commandBuffer = GraphicsDevice.AcquireCommandBuffer();
		var swapchainTexture = commandBuffer.AcquireSwapchainTexture(MainWindow);

		if (swapchainTexture != null)
		{
			RenderCommandLists(commandBuffer, swapchainTexture, drawDataPtr, io);
		}

		GraphicsDevice.Submit(commandBuffer);
	}

	protected override void Destroy()
	{

	}

	private unsafe void UpdateImGuiBuffers(ImDrawDataPtr drawDataPtr)
	{
		if (drawDataPtr.TotalVtxCount == 0) { return; }

		var commandBuffer = GraphicsDevice.AcquireCommandBuffer();

		if (drawDataPtr.TotalVtxCount > VertexCount)
		{
			ImGuiVertexBuffer?.Dispose();

			VertexCount = (uint)(drawDataPtr.TotalVtxCount * 1.5f);
			ImGuiVertexBuffer = MoonWorks.Graphics.Buffer.Create<Position2DTextureColorVertex>(
				GraphicsDevice,
				BufferUsageFlags.Vertex,
				VertexCount
			);
		}

		if (drawDataPtr.TotalIdxCount > IndexCount)
		{
			ImGuiIndexBuffer?.Dispose();

			IndexCount = (uint)(drawDataPtr.TotalIdxCount * 1.5f);
			ImGuiIndexBuffer = MoonWorks.Graphics.Buffer.Create<ushort>(
				GraphicsDevice,
				BufferUsageFlags.Index,
				IndexCount
			);
		}

		uint vertexOffset = 0;
		uint indexOffset = 0;

		for (var n = 0; n < drawDataPtr.CmdListsCount; n += 1)
		{
			var cmdList = drawDataPtr.CmdLists[n];

			commandBuffer.SetBufferData<Position2DTextureColorVertex>(
				ImGuiVertexBuffer,
				cmdList.VtxBuffer.Data,
				vertexOffset,
				(uint)cmdList.VtxBuffer.Size
			);

			commandBuffer.SetBufferData<ushort>(
				ImGuiIndexBuffer,
				cmdList.IdxBuffer.Data,
				indexOffset,
				(uint)cmdList.IdxBuffer.Size
			);

			vertexOffset += (uint)cmdList.VtxBuffer.Size;
			indexOffset += (uint)cmdList.IdxBuffer.Size;
		}

		GraphicsDevice.Submit(commandBuffer);
	}

	private void RenderCommandLists(CommandBuffer commandBuffer, Texture renderTexture, ImDrawDataPtr drawDataPtr, ImGuiIOPtr ioPtr)
	{
		var view = Matrix4x4.CreateLookAt(
			new Vector3(0, 0, 1),
			Vector3.Zero,
			Vector3.Up
		);

		var projection = Matrix4x4.CreateOrthographicOffCenter(
			0,
			480,
			270,
			0,
			0.01f,
			4000f
		);

		var viewProjectionMatrix = view * projection;

		commandBuffer.BeginRenderPass(
			new ColorAttachmentInfo(renderTexture, MoonWorks.Graphics.Color.CornflowerBlue)
		);

		commandBuffer.BindGraphicsPipeline(ImGuiPipeline);

		var vertexUniformOffset = commandBuffer.PushVertexShaderUniforms(
			Matrix4x4.CreateOrthographicOffCenter(0, ioPtr.DisplaySize.X, ioPtr.DisplaySize.Y, 0, -1, 1)
		);

		commandBuffer.BindVertexBuffers(ImGuiVertexBuffer);
		commandBuffer.BindIndexBuffer(ImGuiIndexBuffer, IndexElementSize.Sixteen);

		uint vertexOffset = 0;
		uint indexOffset = 0;

		for (int n = 0; n < drawDataPtr.CmdListsCount; n += 1)
		{
			var cmdList = drawDataPtr.CmdLists[n];

			for (int cmdIndex = 0; cmdIndex < cmdList.CmdBuffer.Size; cmdIndex += 1)
			{
				var drawCmd = cmdList.CmdBuffer[cmdIndex];

				commandBuffer.BindFragmentSamplers(
					new TextureSamplerBinding(TextureStorage.GetTexture(drawCmd.TextureId), ImGuiSampler)
				);

				var topLeft = Vector2.Transform(new Vector2(drawCmd.ClipRect.X, drawCmd.ClipRect.Y), viewProjectionMatrix);
				var bottomRight = Vector2.Transform(new Vector2(drawCmd.ClipRect.Z, drawCmd.ClipRect.W), viewProjectionMatrix);

				var width = drawCmd.ClipRect.Z - (int)drawCmd.ClipRect.X;
				var height = drawCmd.ClipRect.W - (int)drawCmd.ClipRect.Y;

				if (width <= 0 || height <= 0)
				{
					continue;
				}

				commandBuffer.SetScissor(
					new Rect(
						(int)drawCmd.ClipRect.X,
						(int)drawCmd.ClipRect.Y,
						(int)width,
						(int)height
					)
				);

				commandBuffer.DrawIndexedPrimitives(
					vertexOffset,
					indexOffset,
					drawCmd.ElemCount / 3,
					vertexUniformOffset,
					0
				);

				indexOffset += drawCmd.ElemCount;
			}

			vertexOffset += (uint)cmdList.VtxBuffer.Size;
		}

		commandBuffer.EndRenderPass();
	}

	private void BuildFontAtlas()
	{
		var commandBuffer = GraphicsDevice.AcquireCommandBuffer();

		var io = ImGui.GetIO();

		io.Fonts.GetTexDataAsRGBA32(
			out System.IntPtr pixelData,
			out int width,
			out int height,
			out int bytesPerPixel
		);

		var fontTexture = Texture.CreateTexture2D(
			GraphicsDevice,
			(uint)width,
			(uint)height,
			TextureFormat.R8G8B8A8,
			TextureUsageFlags.Sampler
		);

		commandBuffer.SetTextureData(fontTexture, pixelData, (uint)(width * height * bytesPerPixel));

		GraphicsDevice.Submit(commandBuffer);

		io.Fonts.SetTexID(fontTexture.Handle);
		io.Fonts.ClearTexData();

		TextureStorage.Add(fontTexture);
	}
}
