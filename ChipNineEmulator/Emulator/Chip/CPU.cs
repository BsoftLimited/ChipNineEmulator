using System;
using ChipNineEmulator.Emulator;
using ChipNineEmulator.Emulator.Chip;

namespace ChipNineEmulator.Emulator.Chip{
	public class CPU{
		private Registers registers;
		private Stack stack;
		private Memory memory;
		private readonly Emulator emulator;
		private ushort pc, I;
		private int stackPointer, soundTimer, delayTimer;
		private bool drawflag;
		
		public CPU(Emulator emulator){
			this.emulator = emulator;
			initailize();
		}
		
		public void initailize(){
			registers = new Registers();
			stack = new Stack();
			memory = new Memory();
			pc = 0x200;
			I = 0x000;
			stackPointer = 0x0000;
			soundTimer = 0x0000;
			delayTimer = 0x0000;
			drawflag = false;
		}
		
		public void loadProgram(byte[] data){
			memory.loadProgram(data);
		}
		
		public void run(){
			uint opcode = ((uint)(memory.getMemory(pc) << 8) | memory.getMemory(pc+1));
			var init=new processOpcode(this,opcode);
			//Console.WriteLine(Utils.toHex ((int)opcode));
			switch(opcode & 0xF000){
				case 0x0000:
					switch(opcode & 0x000F){
						case 0x0000:
							//clear the screen
							emulator.clearScreen();
							pc+=2;
							drawflag=true;
							break;
						case 0x000E:
							//returns from subroutine
							--stackPointer;
							pc=stack.get(stackPointer);
							pc+=2;
							break;
						default:
							Console.WriteLine("Invalide commande "+ Utils.toHex((int)opcode));
							break;
					}
					break;
				case 0x1000:
					//jump to memory opcode and 0x0FFF
					pc = (ushort)(opcode & 0x0FFF);
					break;
				case 0x2000:
					//calls subroutine at opcode and 0x0FFf
					stack.add(stackPointer,pc);
					++stackPointer;
					pc = (ushort)(opcode & 0x0FFF);
					break;
				case 0x3000:
					//skip next instruction if vx equals kk
					if(init.vx.Equals(init.kk)){pc+=2;}
					pc+=2;
					break;
				case 0x4000:
					//skip next instruction if vx  not equals kk
					if(!init.vx.Equals(init.kk)){pc+=2;}
					pc+=2;
					break;
				case 0x5000:
                	//skip next instruction if vx equals vy
                	if(init.vx.Equals(init.vy)){pc+=2;}
                	pc+=2;
                	break;
                case 0x6000:
                	//set vx=kk
                	init.setRegisterVx(init.kk);
                	pc+=2;
                	break;
                case 0x7000:
                	// 7XNN - Adds NN to VX.
					init.setRegisterVx((byte)((init.vx + init.kk) & 0x00FF));
                	pc+=2;
                	break;
                case 0x8000:
                	switch(opcode & 0x000F){
                		case 0x0000 :
                        	//set vx to vy
                        	init.setRegisterVx(init.vy);
                        	pc+=2;
                        	break;
                        case 0x0001:
                        	//vx=vx or vy
							init.setRegisterVx((byte)(init.vx | init.vy));
                        	pc+=2;
                        	break;
                        case 0x0002:
                        	//vx=vx and vy
							init.setRegisterVx((byte)(init.vx & init.vy));
                        	pc+=2;
                        	break;
                        case 0x0003:
                        	//vx=vx xor xy
                        	init.setRegisterVx((byte)(init.vx ^ init.vy));
                        	pc+=2;
                        	break;
                        case 0x0004:
                        	//vx=vx+vy vf=carry
                        	init.setRegisterVF((byte)(((init.vx + init.vy) & 0x0F00)>>8));
							init.setRegisterVx((byte)((init.vx + init.vy) & 0x00FF));
							pc+=2;
							break;
						case 0x0005:
                        	//vx=vx-vy vf-not borrow  if vx>vy vf=1 else vf=0
                        	if(init.vx>init.vy){
                            	init.setRegisterVF(0x0001);
                        	}else{
                            	init.setRegisterVF(0x0000);
                        	}
							init.setRegisterVx((byte)((init.vx - init.vy) & 0x00FF));
                        	pc+=2;
                        	break;
                        case 0x0006:
                        	//set vx=vx shr 1
							init.setRegisterVF((byte)(init.vx & 0x1));
							init.setRegisterVx((byte)(init.vx >> 1));
                        	pc+=2;
                        	break;
                        case 0x0007:
                        	//vx=vy-vx vf-not borrow  if vy>vx vf=1 else vf=0
                        	if(init.vy>init.vx){
                            	init.setRegisterVF(0x0001);
                        	}else{
                            	init.setRegisterVF(0x0000);
                        	}
							init.setRegisterVx((byte)(init.vy - init.vx));
                        	pc+=2;
                        	break;
                        case 0x000E:
                        	//set vx=vx shl 1
							init.setRegisterVF((byte)(init.vx >> 7));
							init.setRegisterVx((byte)(init.vx << 1));
                        	pc+=2;
                        	break;
                        default:
							Console.WriteLine("Invalide commande "+ Utils.toHex((int)opcode));
							break;
                	}
					break;
                case 0x9000:
                	//skip next instruction if vx not equals vy
                	if(!init.vx.Equals(init.vy)){pc+=2;}
                	pc+=2;break;
                case 0xA000:
                	//set i=nnn
					I = init.nnn;
                	pc+=2;
                	break;
                case 0xB000:
                	//jump to location nnn +v0
					pc = (ushort)(init.nnn + init.getRegisterVN(0));
					break;
				case 0xC000:
                	//vx= random byte and kk
                	int rand=new Random().Next(0,0x100);
					init.setRegisterVx((byte)(rand & init.kk));
                	pc+=2;
                	break;   
                case 0xD000:
                	//display n-byte sprite stating at memory location I at (vx,Vy), set vf=collision
                	byte height = init.n;
                	int pixel;
                	init.setRegisterVF(0);
                	for (int yline=0; yline<height; yline++){
                    	pixel=memory.getMemory(I +yline);
                    	for(int xline=0;xline<8;xline++){
                        	if(!(pixel & (0x80 >> xline)).Equals(0)){
                            	if(emulator.getPixel(init.vx + xline, init.vy +yline).Equals(1)) {
                                	init.setRegisterVF(1);
                            	}
                            	emulator.setPixel(init.vx +xline, init.vy + yline,1);
                        	}
                    	}
                	}
                	drawflag = true;
                	pc+=2;
                	break;
                case 0xE000:
                	switch(opcode & 0x000F){
                		case 0x000E:
                        	//skip next instruction if keys with the value of vx is passed
                        	if(!init.vx.Equals(0)){pc+=2;}
                        	pc+=2;
                        	break;
                        case 0x0001:
                        	//skip next instruction if keys with the value of vx is not passed
                        	if(init.vx.Equals(0)){pc += 2;}
                        	pc+=2;
                        	break;
                    }
					break;
                case 0xF000:
                	switch(opcode & 0x00FF){
                		case 0x0007:
                        	//set vx = delay timer table
							init.setRegisterVx((byte)(delayTimer & 0xFF));
                        	pc+=2;
                        	break;
                        case 0x000A:
                        	//wait for a key press,store the value of the key in vx
                        	bool keyPress = false;
                        	for(int i=0;i<0x10;i++){
                        		if(!Keys.keys[i].Equals(0)) {
									Console.WriteLine("key pressed");
                                	init.setRegisterVx((byte)i);
                                	keyPress = true;
									Keys.ResetKeys();
                            	}
                        	}
                        	// If we didn't received a keypress, skip this cycle and try again.
							if (!keyPress) {
								return;
							}
                        	pc+=2;
							break;
                        case 0x0015:
                        	//set delay timer to vx
                        	delayTimer=init.vx;
                        	pc+=2;
                        	break;
                        case 0x0018:
                        	//set sound timer to vx
                        	soundTimer=init.vx;
                        	pc+=2;
                        	break;
                        case 0x001E:
                        	//set I=I+vx
							I = (ushort)(I + init.vx);
                        	pc+=2;
                        	break;
                        case 0x0029:
                        	//set I=location of sprite for digit vx
							I = (ushort)(init.vx * 0x5);
                        	pc+=2;
                        	break;
                        case 0x0033:
                        	//store BCD representation of vx in memory locations I ,I+1, and I+2
							memory.setMemory(I, (byte)(init.vx / 100));
							memory.setMemory(I + 1, (byte)((init.vx % 100) / 10));
							memory.setMemory(I + 2, (byte)(init.vx % 10));
                        	pc+=2;
                        	break;
                        case 0x0055:
                        	//store Register v0 through vx in memory starting at location I
                        	for (int i=0;i<init.vx;i++){
                            	init.fillMemoryFromRegister(I + i,i);
                        	}
                        	pc+=2;
							break;
                        case 0x0065:
                        	//store Register v0 through vx from memory starting at location I
                        	for (int i=0;i<init.vx;i++){
                            	init.fillRegiterFromMemory(I+i,i);
                        	}
                        	pc+=2;
                        	break;
                        default:
							Console.WriteLine("Invalide commande "+ Utils.toHex((int)opcode));
							break;
					}
					break;
                default:
					Console.WriteLine("Invalide commande "+ Utils.toHex((int)opcode));
					break;
			}
        	// Update timers
        	if(delayTimer > 0){
            	--delayTimer;
            }
        	if(soundTimer > 0) {
        		if(soundTimer == 1){
                	Console.WriteLine("BEEP!\n");
                }
            	--soundTimer;
        	}
        	
        	if(drawflag){
				emulator.draw();
				drawflag = false;
        	}
		}
		
		private class processOpcode{
			public byte vx, vy, kk, n;
			public ushort nnn;
			CPU cpu;
			uint opcode;
        
			public processOpcode(CPU cpu,uint opcode){
				this.cpu = cpu;
				this.opcode = opcode;
				vx = cpu.registers.getRegister("V" + Utils.toHex((int)((opcode & 0x0F00) >> 8)));
				vy = cpu.registers.getRegister("V" + Utils.toHex((int)((opcode & 0x00F0) >> 4)));
				kk = (byte)(opcode & 0x00FF);
				nnn = (ushort)(opcode & 0x0FFF);
				n = (byte)(opcode & 0x000F);
			}
			
			public void setRegisterVx(byte value){
				cpu.registers.setRegister("V" + Utils.toHex((int)((opcode & 0x0F00) >> 8)), value);
			}
			
			public void setRegisterVy(byte value){
				cpu.registers.setRegister("V" + Utils.toHex((int)((opcode & 0x00F0) >> 4)), value);
			}
			
			public void setRegisterVF(byte value){
				cpu.registers.setRegister("VF", value);
			}
			
			public void setRegisterVN(int n, byte value){
				cpu.registers.setRegister("V" + Utils.toHex(n), value);
			}
			
			public byte getRegisterVN(int n){
				return cpu.registers.getRegister("V" + Utils.toHex(n));
			}
			
			public void fillRegiterFromMemory(int memoryLocation, int registerLocation){
				cpu.registers.setRegister("V" + Utils.toHex(registerLocation), cpu.memory.getMemory(memoryLocation));
			}

			public void fillMemoryFromRegister(int memoryLocation, int registerLocation){
				cpu.memory.setMemory(memoryLocation, cpu.registers.getRegister("V" + Utils.toHex(registerLocation)));
			}
		}
	}
	
	public class Registers{
		private readonly Register[] registers;
		public Registers(){
			registers = new Register[0x10];
			for (int i = 0; i < 0x10; i++) {
				registers[i] = new Register("V" + Utils.toHex(i));
			}
		}
		
		public byte getRegister(String name){
			for (int i = 0; i < 0x10; i++) {
				if (registers[i].name.Equals(name)) {
					return registers[i].register;
				}
			}
			Console.WriteLine("Register not found");
			return 0x000;
		}
		
		public void setRegister(String name, Byte value){
			for (int i = 0; i < 0x10; i++) {
				if (registers[i].name.Equals(name)) {
					registers[i].register = value;
				}
			}
		}
		
		class Register{
			public Byte register;
			public String name;
			public Register(String name){
				register = 0x0000;
				this.name = name;
			}
		}
	}
	
	class Stack{
		private readonly ushort[] stack;
		public Stack()
		{
			stack = new ushort[0x10];
		}
		
		public void add(int index, ushort value){
			stack[index] = value;
		}
		
		public ushort get(int index){
			return (ushort)stack[index];
		}
	}
	
	class Memory{
		private readonly byte[] memories;
		public Memory(){
			memories = new byte[0x1000];
			for (int i = 0; i < memories.Length; i++) {
				memories[i] = 0x00;
			}
			
			for (int i = 0; i < 79; i++) {
				memories[i] = Keys.FONT[i];
			}
		}
		
		public byte getMemory(int index){
			return (byte)memories[index];
		}
		
		public void setMemory(int index, byte value){
			memories[index] = value;
		}
		
		public void loadProgram(byte[] data){
			for (int i = 0; i < data.Length; i++) {
				memories[i + 512] = (byte)(data[i] & 0x0FFF);
			}
		}
	}
}
