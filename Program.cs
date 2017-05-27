using System;
using System.Configuration;
using System.Diagnostics;
using System.Collections.Generic;


namespace UnitronicsDatalogger{

	class MainClass{
		public static ConfigHandler confH;
		public static SQLHandler sqlH;
		public static PLCHandler plcH;

		public static void Main(string[] args){
			confH = new ConfigHandler();
			sqlH = new SQLHandler(confH);
			plcH = new PLCHandler(confH);
			Random rnd = new Random();
			Stopwatch sw = new Stopwatch();
			List<Definition> toGet=new List<Definition>();
			List<Definition> toLog=new List<Definition>();
			while(true){
				Logger.Log("New iteration");
				sw.Start();
				sqlH.updateDefinitions();
				sqlH.createTables();
				toLog.Clear();
				toGet.Clear();
				for(int i = 0; i < sqlH.getDefinitionsCount(); i++){
					Definition def = sqlH.getDefintion(i);
					toGet.Add(def);
				}
				toLog = plcH.getValues(toGet);
				for(int i = 0; i < toLog.Count; i++){
					Definition def = toLog[i];
					Logger.Log("Logged id " + def.id);
					string time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:00");
					sqlH.sendRaw("INSERT INTO `unitronics`.`val_" + def.id + "` (`datetime`, `value`) VALUES ('" + time + "', '" + def.value + "');");
					sqlH.sendRaw("UPDATE definitions SET lastTime = '"+time+"' WHERE id = "+def.id+";");
				}
				sw.Stop();
				Logger.Log("Took "+sw.ElapsedMilliseconds+"ms. Waiting for next iteration...");
				sw.Reset();
				System.Threading.Thread.Sleep((60 - DateTime.UtcNow.Second) * 1000);
			}
		}
	}
}
