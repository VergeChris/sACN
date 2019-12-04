﻿using Haukcode.sACN;
using Haukcode.sACN.Model;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace Haukcode.sACN.ConsoleExample
{
    public class Program
    {
        private static readonly Guid acnSourceId = new Guid("{B32625A6-C280-4389-BD25-E0D13F5B50E0}");
        private static readonly string acnSourceName = "DMXPlayer";

        public static void Main(string[] args)
        {
            Listen();
        }

        static void Listen()
        {
            var client = new SACNClient(
                senderId: acnSourceId,
                senderName: acnSourceName,
                localAddress: SACNCommon.GetFirstBindAddress());

            client.OnError.Subscribe(e =>
            {
                Console.WriteLine($"Error! {e.Message}");
            });

            //listener.OnReceiveRaw.Subscribe(d =>
            //{
            //    Console.WriteLine($"Received {d.Data.Length} bytes from {d.Host}");
            //});

            double last = 0;
            client.OnPacket.Subscribe(d =>
            {
                Listener_OnPacket(d.TimestampMS, d.TimestampMS - last, d.Packet);
                last = d.TimestampMS;
            });

            client.StartReceive();
            client.JoinDMXUniverse(1);
            client.JoinDMXUniverse(2);

            while (true)
            {
                client.SendMulticast(1, new byte[] { 1, 2, 3, 4, 5 });

                Thread.Sleep(500);
            }
        }

        private static void Listener_OnPacket(double timestampMS, double sinceLast, SACNPacket e)
        {
            Console.Write($"+{sinceLast:N2}\t");
            Console.Write($"Packet from {e.SourceName}\tu{e.UniverseId}\ts{e.SequenceId}\t");
            Console.WriteLine($"Data {string.Join(",", e.DMXData.Take(16))}...");
        }
    }
}
