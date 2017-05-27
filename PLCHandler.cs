using System;
using System.Collections.Generic;
using Unitronics;
using Unitronics.ComDriver;
using Unitronics.ComDriver.Messages;
using Unitronics.ComDriver.Messages.DataRequest;

namespace UnitronicsDatalogger{

	public class PLCHandler{

		bool isEnabled;
		string IP;
		int port;

		Ethernet eth;
		PLC plc;

		public PLCHandler(ConfigHandler cfgH){
			isEnabled = cfgH.getSetting("enablePLC") == "1";
			IP = cfgH.getSetting("uniIP");
			port = Int32.Parse(cfgH.getSetting("uniPORT"));
		}

		private bool connect(){
			return connect (this.IP, this.port);
		}

		private bool connect(string IP,int port){
			Logger.Log("Connecting...");
			try{
				eth = new Ethernet (IP, port, EthProtocol.TCP, 3, 3000);
				PLCFactory.WorkWithXmlInternally = true;
				plc = PLCFactory.GetPLC (eth, 0);
				Logger.Log (eth.Connected ? "Connected" : "Unable to connect");
				return eth.Connected;
			}catch(Exception e){
				Logger.Log ("Error connecting: "+e.Message);
				return false;
			}
		}

		private void disconnect(){
			Logger.Log ("Disconnecting...");
			plc.Disconnect();
			Logger.Log ("Disconnected");
		}

		public bool isConnected(){
			return eth.Connected;
		}

		public List<Definition> getValues(List<Definition> defs){
			if(!isEnabled){return randomVals(defs);}
			Logger.Log("Getting PLC values");
			ReadWriteRequest[] rw=new ReadWriteRequest[defs.Count];
			for(int i=0;i<defs.Count;i++){
				rw[i] = new ReadOperands() {
					OperandType = OperandTypes.MI,
					NumberOfOperands = 1,
					StartAddress = Convert.ToUInt16(defs[i].MI)
				};
			}
			try{
				connect();
				plc.ReadWrite(ref rw);
				disconnect();
			}catch(Exception e){
				Logger.Error("PLC RW exception: " + e.Message + "\n" + e.StackTrace);
			}

			for(int i = 0; i < rw.Length; i++){
				try{
					object[] values = (object[])(rw [i].ResponseValues);
					defs[i].value=(double)Double.Parse (values [0].ToString ()) / (double)(defs[i].divider);
				}catch(Exception e){
					Logger.Error("RW exception: "+i+"\n"+e.Message+"\n"+e.StackTrace);
				}
			}
			return defs;
		}
			
		/*
		 * Meetod suvaliste väärtuste tagastamiseks testimise eesmärgil
		 * Kasutatakse kui seadetest on enablePLC=0
		 */
		private List<Definition> randomVals(List<Definition> defs){
			Random rnd = new Random();
			for(int i = 0; i < defs.Count; i++){
				defs[i].value = rnd.Next(18, 25);
			}
			return defs;
		}
	}
}

