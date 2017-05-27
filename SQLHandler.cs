using System;
using System.Collections.Generic;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace UnitronicsDatalogger{

	public class SQLHandler{

		private string server;
		private string database;
		private string uid;
		private string password;
		private MySqlConnection connection;
		private List<Definition> definitions; //Kõik andmebaasist alla laetud definitsioonid

		public SQLHandler(ConfigHandler cfgh){
			loadSettings(cfgh);
			string connectionString = "Server=" + this.server + ";" + "Database=" + this.database + ";" + "Uid=" + this.uid + ";" + "Pwd=" + this.password + ";";
			//Logger.Log(connectionString);
			connection = new MySqlConnection(connectionString);
			definitions = new List<Definition>();
		}
			
		/*
		 * Laeb seadete failist seaded
		 * @param ConfigHandler cfgh
		 */
		private void loadSettings(ConfigHandler cfgh){
			try{
				this.server = cfgh.getSetting("sqlServer");
				this.database = cfgh.getSetting("sqlDatabase");
				this.uid = cfgh.getSetting("sqlUsername");
				this.password = cfgh.ROT13(cfgh.getSetting("sqlPassword"));
				Logger.Log("Loaded SQL settings.");
			} catch(NullReferenceException nre){
				Logger.Log("Error loading SQL settings. " + nre.Message);
			}
		}

		/*
		 * Avab uue MySQL ühenduse
		 * @return bool tõene kui ühendus õnnestus
		 */
		private bool openConnection(){
			try{
				connection.Open();
				return true;
			} catch(MySqlException ex){
				switch(ex.Number){
				case 0:
					Logger.Log("Cannot connect to server.  Contact administrator");
					break;

				case 1045:
					Logger.Log("Invalid username/password, please try again");
					break;
				default:
					Logger.Log("Unknown MySQL exception: " + ex.Number);
					break;
				}
				return false;
			}
		}

		/*
		 * avab ühenduse andmebaasiga ja saadab SQL päringu andmebaasi ning lõpus sulgeb andmebaasi ühenduse
		 * @param string query päringu sisu
		 */
		public void sendRaw(string query){
			try{
				if(this.openConnection() == true){
					MySqlCommand cmd = new MySqlCommand(query, connection);
					cmd.ExecuteNonQuery();
					this.closeConnection();
				}
			} catch(MySqlException mse){
				Logger.Log("SQL Error: " + query + "\n" + mse.Message + "\n" + mse.StackTrace);
			}
		}

		/*
		 * Sulgeb andmebaasiga ühenduse
		 * @return bool tagastab, kas sulgemine õnnestus
		 */
		private bool closeConnection(){
			try{
				connection.Close();
				return true;
			} catch(MySqlException ex){
				Logger.Log("SQL Error: " + ex.Message);
				return false;
			}
		}

		/*
		 * Laeb alla SQL databaasist kõikide definitsioonide sisud
		 */
		public void updateDefinitions(){
			string query = @"SELECT `MI`, `id`, `divider` FROM `definitions` WHERE UNIX_TIMESTAMP(`lastTime`)+60*`interval` <= UNIX_TIMESTAMP(NOW())";
			MySqlDataReader rdr = null;
			try{
				if(this.openConnection() == true){
					MySqlCommand cmd = new MySqlCommand(query, connection);
					rdr = cmd.ExecuteReader();
					this.definitions.Clear();
					while(rdr.Read()){
						Definition def = new Definition();
						def.MI = rdr.GetInt32(0);
						def.id = rdr.GetInt32(1);
						def.divider = rdr.GetInt32(2);
						this.definitions.Add(def);
					}
				}
			} catch(MySqlException mse){
				Logger.Log("SQL Error: " + query + "\n" + mse.Message + "\n" + mse.StackTrace);
			} finally{
				if(rdr != null){
					rdr.Close();
				}
				this.closeConnection();
			}
				
		}

		/*
		 * Teeb MySQL databaasi uue tabeli nimega 'val_[id]'
		 */
		public void createTables(){
			Logger.Log("Creating tables");
			foreach(Definition def in definitions){
				sendRaw(@"CREATE TABLE IF NOT EXISTS `val_" + def.id + "` (`datetime` datetime NOT NULL,`value` decimal(7,3) NOT NULL,KEY `datetime` (`datetime`)) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;");
			}
		}

		public int getDefinitionsCount(){
			return this.definitions.Count;
		}
		public Definition getDefintion(int i){
			return this.definitions[i];
		}

		public void setDefinitions(Definition def,int i){
			this.definitions[i]= def;
		}

		public List<Definition> getDefinitions(){
			return this.definitions;
		}
	}
}

