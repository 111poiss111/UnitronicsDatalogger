using System;

namespace UnitronicsDatalogger{

	public class Logger{

		public static void Log(string str){
			Console.WriteLine(getDateTime()+"[INFO]" + str);
		}

		public static void Error(string str){
			Console.WriteLine(getDateTime()+"[ERROR]"+ str);
		}

		public static string getDateTime(){
			return "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]";
		}
	}
}

