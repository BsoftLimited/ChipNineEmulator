/*
 * Created by SharpDevelop.
 * User: okelekele kiensue
 * Date: 11/20/2018
 * Time: 1:59 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace ChipNineEmulator.Emulator{
	public static class Utils{
		public static String toHex(int number){
			String hex = "";
			while(number>0){
				switch(number%16){
					case 10:
						hex = "A"+hex;
						break;
					case 11:
						hex = "B"+hex;
						break;
					case 12:
						hex = "C"+hex;
						break;
					case 13:
						hex = "D"+hex;
						break;
					case 14:
						hex = "E"+hex;
						break;
					case 15:
						hex = "F"+hex;
						break;
					default:
						hex = (number % 16).ToString() + hex;
						break;
				}
				number /= 16;
			}
			return hex;
		}
		
		public static float toRadians(float degrees){
			return (float)(Math.PI * (degrees / 180));
		}
		
		public static float[] getBufferData(Vector point){
			Vector next = point.add(new Vector(1f, 1f));
			float[] data = {
				point.getX(), point.getY(), 0,
				next.getX(), point.getY(), 0,
				next.getX(), next.getY(), 0,
				point.getX(), next.getY(), 0
			};
			return data;
		}
	}
	
	public class Vector{
		private float x;
		private float y;
		
		public Vector(float x,float y){
			this.x = x;
			this.y = y;
		}
		
        public float getX(){
            float init=x*10.0f;
            return (init-320.0f)/320.0f;
        }
		
        public float getY() {
            float init=y*10.0f;
            return (init-160.0f)/160.0f;
        }
		
		public float getRawX(){
            return x;
        }
		
        public float getRawY() {
            return y;
        }
		
		public float lenght(){
			return (float)Math.Sqrt(Math.Pow(this.x, 2.0) + Math.Pow(this.y, 2.0));
		}
		
		public Vector add(Vector vector){
			return new Vector(vector.getRawX() + this.x, vector.getRawY() + this.y);
		}
		
		public Vector subtract(Vector vector){
			return new Vector(this.x - vector.getRawX(), this.y - vector.getRawY());
		}
		
		public float dot(Vector vector){
			return (vector.getRawX() * this.x) + (vector.getRawY() * this.y);
		}
		
		public Vector multiply(float unit){
			return new Vector(unit * this.x, unit * this.y);
		}
		
		public Vector normalize(){
			float length = this.lenght();
			this.x /= length;
			this.y /= length;
			return this;
		}
		
		public Vector rotate(float angle){
			float rad = Utils.toRadians(angle);
			float cos = (float)Math.Cos((double)rad);
			float sin = (float)Math.Sin((double)rad);
			return new Vector((this.x * cos - this.y * sin), (this.x * sin + this.y * cos));
		}
    }
}
