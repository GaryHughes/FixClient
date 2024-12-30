/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: Program.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FixPerformanceTest;

class Program
{
    delegate void ActionDelegate();

    static void TimeOperation(ActionDelegate action, string label)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        action();
        stopwatch.Stop();
        Console.WriteLine("{0} - {1}", stopwatch.Elapsed, label);
    }

    static void Main()
    {
        TimeOperation(TestMessageFieldAccess, "Message Field Access");
        TimeOperation(TestReadHistoryFile, "Read History File");
    }

    static void TestMessageFieldAccess()
    {
        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=28635=849=ITGHK56=KODIAK_KASFQA34=163357=kasfqa52=20091023-05:40:1637=712-217=420=039=055=649707154=238=100000032=031=0.00000014=06=0.00000011=296.2.240=160=20091023-05:40:1659=047=A30=DMA15=KRW6005=ALT=18500TOT=1256276416111=09886=0.0000009887=09912=09911=010=135");
        var message = new Fix.Message(data);
        int[] tags = new int[message.Fields.Count];
        for (int index = 0; index < tags.Length; ++index)
        {
            tags[index] = message.Fields[index].Tag;
        }
        Console.WriteLine("length = {0}", tags.Length);
        var random = new Random();
        for (int i = 0; i < 100000; ++i)
        {
            int tag = tags[random.Next(tags.Length)];
            _ = message.Fields.Find(tag);
        }
    }

    static void TestReadHistoryFile()
    {
        const string filename = @"C:\Users\geh\Downloads\Latha\FIX_ExchangeSimulator20150113.history";

        var messages = new Fix.MessageCollection();

        try
        {
            long count = 0;
            long exceptions = 0;
            using (var stream = new FileStream(filename, FileMode.Open))
            using (var reader = new Fix.Reader(stream))
            {
                for (; ; )
                {
                    try
                    {
                        Fix.Message? message = reader.ReadLine();

                        if (message == null)
                            break;

                        messages.Add(message);
                    }
                    catch (Exception)
                    {
                        ++exceptions;
                        reader.DiscardLine();
                        continue;
                    }
                    ++count;
                }
            }
            Console.WriteLine("{0} / {1}", count, exceptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine("MESSAGES - {0}", messages.Count);

        var book = new Fix.OrderBook();

        try
        {
            foreach (var message in messages)
            {
                book.Process(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        Console.WriteLine("ORDERS - {0}", book.Orders.Count);
    }
}

