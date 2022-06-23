using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.IO;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace WOL
{
    class Program
    {
        static string PathToSettings = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        static Dictionary<string, string> pcs = new Dictionary<string, string>();
        static MqttClient client = new MqttClient(IPAddress.Loopback);
        static void Main(string[] args)
        {
            try
            {
                StreamReader sr = new StreamReader(PathToSettings);
                JArray arr = JArray.Parse(sr.ReadToEnd());
                sr.Close();
                for (int i = 0; i < arr.Count; i++)
                {
                    pcs.Add((string)arr[i]["name"], (string)arr[i]["mac"]);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine($"{DateTime.Now.ToString().Replace('/', '.')}\t[ERROR]\t{exp.Message}\t{exp.StackTrace}");
                return;
            }
            // register to message received 
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId, "Krypt0n", "dima160597");

            // subscribe to the topic "/home/temperature" with QoS 2 
            client.Subscribe(new string[] { "/home/rooms/cabinet/pc" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            Console.WriteLine($"{DateTime.Now.ToString().Replace('/','.')}\tStarted!");
            while (true) Thread.Sleep(100) ;
        }

        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string msg = Encoding.UTF8.GetString(e.Message);
            //Console.WriteLine($"{DateTime.Now.ToString()}\tMessage arrived {msg}");
            if (pcs.ContainsKey(msg))
            {
                SendWakeOnLan(PhysicalAddress.Parse(pcs[msg]));
                Console.WriteLine($"{DateTime.Now.ToString().Replace('/', '.')}\tSended WOL on {msg} via {pcs[msg]} MAC!");
                client.Publish("/home/rooms/cabinet/pc", Encoding.UTF8.GetBytes(" "), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            }
            
        }

        static void SendWakeOnLan(PhysicalAddress target)
        {
            var header = Enumerable.Repeat(byte.MaxValue, 6);
            var data = Enumerable.Repeat(target.GetAddressBytes(), 16).SelectMany(mac => mac);

            var magicPacket = header.Concat(data).ToArray();

            using var client = new UdpClient();

            client.Send(magicPacket, magicPacket.Length, new IPEndPoint(IPAddress.Broadcast, 9));
        }
    }
}
