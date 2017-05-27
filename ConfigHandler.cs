using System;
using System.Configuration;

namespace UnitronicsDatalogger{

	public class ConfigHandler{

		public ConfigHandler(){
		}

		/*
		 * Returns set setting for specific key
		 * @param string key Key of specific setting
		 * @return string|null Setting with specific key or null if it doesn't exist
		 */
		public string getSetting(string key){
			if(ConfigurationManager.AppSettings[key] == null){
				throw new NullReferenceException("Key: '" + key + "' doesn't exist.");
			}
			return ConfigurationManager.AppSettings[key];
		}

		/*
		 * Very cryptography. Much unhackable. Wow
		 * @param string input String to (en/de)crypt
		 * @return string (De)crypted string
		 */
		public string ROT13(string input){
			char[] array = input.ToCharArray();
			for(int i = 0; i < array.Length; i++){
				int number = (int)array[i];

				if(number >= 'a' && number <= 'z'){
					if(number > 'm'){
						number -= 13;
					} else{
						number += 13;
					}
				} else if(number >= 'A' && number <= 'Z'){
					if(number > 'M'){
						number -= 13;
					} else{
						number += 13;
					}
				}
				array[i] = (char)number;
			}
			return new string(array);
		}
	}
}

