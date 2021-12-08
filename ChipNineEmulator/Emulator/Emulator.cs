using System;
using System.IO;
using ChipNineEmulator.Emulator.Chip;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ChipNineEmulator.Emulator{
	public class Emulator: GameWindow{
		private byte[,] display;
		private Sprite[,] sprites;
		private CPU cpu;
		private bool drawScreen;
		
		public Emulator() : base(0x40*0xA,0x20* 0xA){
			this.Title = "Chip-Nine Emulator";
			initailize();
			loadRom("C:/projects/chipNineEmulator/src/roms/pong2.c8");
			this.Run(1 / 500f);
			drawScreen = false;
		}
		
		private void initailize(){
			cpu = new CPU(this);
			display = new byte[0x20, 0x40];
			sprites = new Sprite[0x20, 0x40];
		}
		
		public void clearScreen(){
			for (int height = 0; height < 32; height++) {
				for (int width = 0; width < 64; width++) {
					display[height, width] = 0;
				}
			}
		}
		
		public void draw(){
			drawScreen = true;
		}
		
		protected override void OnLoad(EventArgs e){
			base.OnLoad(e);
			GL.ClearColor(0.2f, 0.0f, 0.2f, 1f);
			GL.FrontFace(FrontFaceDirection.Ccw);
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);
			GL.Enable(EnableCap.DepthTest);
			for(int height=0;height<32;height++){
				for(int width=0;width<64;width++){
					display[height, width]=0;
					sprites[height, width]=new Sprite(width,height);
                }
            }
		}
		
		protected override void OnKeyPress(KeyPressEventArgs e){
			base.OnKeyPress(e);
			try{
				if(e.KeyChar.Equals('i')){
					Keys.keys[2] = 1;
				}else if(e.KeyChar.Equals('k')){
					Keys.keys[4] = 1;
				}else if(e.KeyChar.Equals('j')){
					Keys.keys[6] = 1;
				}else if(e.KeyChar.Equals('m')){
					Keys.keys[8] = 1;
				}
			}catch(Exception ex){
				
			}
		}
		
		protected override void OnUpdateFrame(FrameEventArgs e){
			base.OnUpdateFrame(e);
			cpu.run();
		}
		
		protected override void OnRenderFrame(FrameEventArgs e){
			base.OnRenderFrame(e);
			if (drawScreen) {
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
				for (int height = 0; height < 0x20; height++) {
					for (int width = 0; width < 0x40; width++) {
						if (display[height, width].Equals(1)) {
							GL.BindVertexArray((sprites[0x1F-height, width]).voa);
							GL.DrawArrays(PrimitiveType.Quads, 0, 12);
							GL.BindVertexArray(0);
						}
					}
				}
				this.SwapBuffers();
				drawScreen = false;
			}
		}
		
		public void setPixel(int x,int y,byte value){
			display[y, x] = (byte)(display[y, x] > 0 ? 0 : value);
		}

    	public byte getPixel(int x, int y){
			return display[y, x];
		}
		
		public void loadRom(String path){
			byte[] data=new byte[0];
			try{
				FileStream rom = new FileStream(path, FileMode.Open);
				data=new byte[rom.Length];
				rom.Read(data, 0, (int)rom.Length);
			}catch(FileNotFoundException e){
				Console.WriteLine("file not found");
			}catch(FileLoadException e){
				Console.WriteLine("unable to read rom");
			}finally{
				cpu.loadProgram(data);
			}
		}
		
		private class Sprite{
			public int voa;
			public Sprite(float x,float y){
				voa = GL.GenVertexArray();
				GL.BindVertexArray(voa);
				int vbo = GL.GenBuffer();
				float[] data=Utils.getBufferData(new Vector(x,y));
				int size = data.Length;
				GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
				GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(size * sizeof(float)), data, BufferUsageHint.StaticDraw);
				GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,0, 0);
				GL.EnableVertexAttribArray(0);
				GL.BindVertexArray(0);
			}
		}
	}
}