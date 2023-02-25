/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: SessionTests.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FixClientTests;

[TestClass]
public class SessionTests
{
    [TestMethod]
    public void TestCopyPropertiesFrom()
    {
        var one = new FixClient.Session(new System.Windows.Forms.Control())
        {
            OrderBehaviour = Fix.Behaviour.Initiator
        };

        var two = new FixClient.Session(new System.Windows.Forms.Control())
        {
            OrderBehaviour = Fix.Behaviour.Acceptor
        };

        Assert.AreEqual(Fix.Behaviour.Initiator, one.OrderBehaviour);
        Assert.AreEqual(Fix.Behaviour.Acceptor, two.OrderBehaviour);

        one.CopyPropertiesFrom(two);

        Assert.AreEqual(Fix.Behaviour.Acceptor, one.OrderBehaviour);
        Assert.AreEqual(Fix.Behaviour.Acceptor, two.OrderBehaviour);
    }

    [TestMethod]
    public void TestCloneSession()
    {
        var original = new FixClient.Session(new System.Windows.Forms.Control())
        {
            OrderBehaviour = Fix.Behaviour.Initiator,
            BeginString = Fix.Dictionary.Versions.FIXT_1_1,
            DefaultApplVerId = Fix.Dictionary.Versions.FIX_5_0SP2,
            LogonBehaviour = Fix.Behaviour.Initiator,
            SenderCompId = "SENDER",
            TargetCompId = "TARGET",
            HeartBtInt = 30,
            OutgoingTimestampFormat = Fix.Field.TimestampFormatLong,
            IncomingSeqNum = 1,
            OutgoingSeqNum = 1,
            BrokenNewSeqNo = true,
            TestRequestId = 1,
            FragmentMessages = true,
            AutoSendingTime = true,
            FileName = @"C:\some\path\file.session",
            Behaviour = Fix.Behaviour.Initiator,
            BindHost = "localhost",
            BindPort = 10000,
            Host = "localhost",
            Port = 20000,
            NextClOrdId = 1,
            NextListId = 1,
            NextAllocId = 1,
            NextOrderId = 1,
            NextExecId = 1,
            AutoSetMsgSeqNum = true,
            AutoTotNoOrders = true,
            AutoNoOrders = true,
            AutoListId = true,
            AutoClOrdId = true,
            AutoListSeqNo = true,
            AutoTransactTime = true,
            AutoAllocId = true,
            AutoScrollMessages = true
        };

        var clone = (FixClient.Session)original.Clone();

        clone.OrderBehaviour = Fix.Behaviour.Acceptor;
        clone.BeginString = Fix.Dictionary.Versions.FIX_4_2;
        clone.DefaultApplVerId = Fix.Dictionary.Versions.FIX_4_2;
        clone.LogonBehaviour = Fix.Behaviour.Acceptor;
        clone.SenderCompId = "INITIATOR";
        clone.TargetCompId = "ACCEPTOR";
        clone.HeartBtInt = 60;
        clone.OutgoingTimestampFormat = Fix.Field.TimestampFormatShort;
        clone.IncomingSeqNum = 2;
        clone.OutgoingSeqNum = 2;
        clone.BrokenNewSeqNo = false;
        clone.TestRequestId = 2;
        clone.FragmentMessages = false;
        clone.AutoSendingTime = false;
        clone.FileName = @"D:\other\path\file.session";
        clone.Behaviour = Fix.Behaviour.Acceptor;
        clone.BindHost = "remotehost";
        clone.BindPort = 30000;
        clone.Host = "otherhost";
        clone.Port = 40000;
        clone.NextClOrdId = 4;
        clone.NextListId = 4;
        clone.NextAllocId = 4;
        clone.NextOrderId = 4;
        clone.NextExecId = 4;
        clone.AutoSetMsgSeqNum = false;
        clone.AutoTotNoOrders = false;
        clone.AutoNoOrders = false;
        clone.AutoListId = false;
        clone.AutoClOrdId = false;
        clone.AutoListSeqNo = false;
        clone.AutoTransactTime = false;
        clone.AutoAllocId = false;
        clone.AutoScrollMessages = false;

        Assert.AreEqual(Fix.Behaviour.Initiator, original.OrderBehaviour);
        Assert.AreEqual(Fix.Dictionary.Versions.FIXT_1_1, original.BeginString);
        Assert.AreEqual(Fix.Dictionary.Versions.FIX_5_0SP2, original.DefaultApplVerId);
        Assert.AreEqual(Fix.Behaviour.Initiator, original.LogonBehaviour);
        Assert.AreEqual("SENDER", original.SenderCompId);
        Assert.AreEqual("TARGET", original.TargetCompId);
        Assert.AreEqual(30, original.HeartBtInt);
        Assert.AreEqual(true, original.OutgoingTimestampFormat);
        Assert.AreEqual(1, original.IncomingSeqNum);
        Assert.AreEqual(1, original.OutgoingSeqNum);
        Assert.AreEqual(true, original.BrokenNewSeqNo);
        Assert.AreEqual(1, original.TestRequestId);
        Assert.AreEqual(true, original.FragmentMessages);
        Assert.AreEqual(true, original.AutoSendingTime);
        Assert.AreEqual(@"C:\some\path\file.session", original.FileName);
        Assert.AreEqual(Fix.Behaviour.Initiator, original.Behaviour);
        Assert.AreEqual("localhost", original.BindHost);
        Assert.AreEqual(10000, original.BindPort);
        Assert.AreEqual("localhost", original.Host);
        Assert.AreEqual(20000, original.Port);
        Assert.AreEqual(1, original.NextClOrdId);
        Assert.AreEqual(1, original.NextListId);
        Assert.AreEqual(1, original.NextAllocId);
        Assert.AreEqual(1, original.NextOrderId);
        Assert.AreEqual(1, original.NextExecId);
        Assert.AreEqual(true, original.AutoSetMsgSeqNum);
        Assert.AreEqual(true, original.AutoTotNoOrders);
        Assert.AreEqual(true, original.AutoNoOrders);
        Assert.AreEqual(true, original.AutoListId);
        Assert.AreEqual(true, original.AutoClOrdId);
        Assert.AreEqual(true, original.AutoListSeqNo);
        Assert.AreEqual(true, original.AutoTransactTime);
        Assert.AreEqual(true, original.AutoAllocId);
        Assert.AreEqual(true, original.AutoScrollMessages);

        Assert.AreEqual(Fix.Behaviour.Acceptor, clone.OrderBehaviour);
        Assert.AreEqual(Fix.Dictionary.Versions.FIX_4_2, clone.BeginString);
        Assert.AreEqual(Fix.Dictionary.Versions.FIX_4_2, clone.DefaultApplVerId);
        Assert.AreEqual(Fix.Behaviour.Acceptor, clone.LogonBehaviour);
        Assert.AreEqual("INITIATOR", clone.SenderCompId);
        Assert.AreEqual("ACCEPTOR", clone.TargetCompId);
        Assert.AreEqual(60, clone.HeartBtInt);
        Assert.AreEqual(false, clone.OutgoingTimestampFormat);
        Assert.AreEqual(2, clone.IncomingSeqNum);
        Assert.AreEqual(2, clone.OutgoingSeqNum);
        Assert.AreEqual(false, clone.BrokenNewSeqNo);
        Assert.AreEqual(2, clone.TestRequestId);
        Assert.AreEqual(false, clone.FragmentMessages);
        Assert.AreEqual(false, clone.AutoSendingTime);
        Assert.AreEqual(@"D:\other\path\file.session", clone.FileName);
        Assert.AreEqual(Fix.Behaviour.Acceptor, clone.Behaviour);
        Assert.AreEqual("remotehost", clone.BindHost);
        Assert.AreEqual(30000, clone.BindPort);
        Assert.AreEqual("otherhost", clone.Host);
        Assert.AreEqual(40000, clone.Port);
        Assert.AreEqual(4, clone.NextClOrdId);
        Assert.AreEqual(4, clone.NextListId);
        Assert.AreEqual(4, clone.NextAllocId);
        Assert.AreEqual(4, clone.NextOrderId);
        Assert.AreEqual(4, clone.NextExecId);
        Assert.AreEqual(false, clone.AutoSetMsgSeqNum);
        Assert.AreEqual(false, clone.AutoTotNoOrders);
        Assert.AreEqual(false, clone.AutoNoOrders);
        Assert.AreEqual(false, clone.AutoListId);
        Assert.AreEqual(false, clone.AutoClOrdId);
        Assert.AreEqual(false, clone.AutoListSeqNo);
        Assert.AreEqual(false, clone.AutoTransactTime);
        Assert.AreEqual(false, clone.AutoAllocId);
        Assert.AreEqual(false, clone.AutoScrollMessages);
    }
}

