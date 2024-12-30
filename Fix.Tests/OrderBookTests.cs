/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: OrderBookTests.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static Fix.Dictionary;

namespace FixTests;

[TestClass]
public class OrderBookTests
{
    [TestMethod]
    public void TestTritonNewAck()
    {
        var orderSingle = new Fix.Message("8=FIX.4.09=14135=D49=HKGATE56=GEHVWAP115=test34=952=20090728-23:34:516005=ASN=PACISS=V3M0tP0tV1tQ0tU211=121=155=RIO54=138=10040=165=AX44=4010=042");
        var ack = new Fix.Message("8=FIX.4.09=16935=849=GEHVWAP56=HKGATE128=test34=1197=N52=20090728-23:34:526=0.00000011=114=017=120=031=0.00000032=037=138=10039=040=144=0.00000054=155=RIO.AX59=010=254");

        var book = new Fix.OrderBook();

        Assert.AreEqual(0, book.Orders.Count);
        Assert.AreEqual(book.Process(orderSingle), Fix.OrderBookMessageEffect.Modified);
        Assert.AreEqual(1, book.Orders.Count);

        Fix.Order order = book.Orders[0];
        Assert.AreEqual(1, order.Messages.Count);
        Assert.AreEqual("RIO", order.Symbol);
        Assert.AreEqual(40m, order.Price);
        Assert.AreEqual(FIX_5_0SP2.Side.Buy, order.Side);

        Assert.AreEqual(book.Process(ack), Fix.OrderBookMessageEffect.Modified);
        Assert.AreEqual(1, book.Orders.Count);

        order = book.Orders[0];
        Assert.AreEqual(2, order.Messages.Count);
        Assert.AreEqual("RIO", order.Symbol);
        Assert.AreEqual(FIX_5_0SP2.Side.Buy, order.Side);
        Assert.AreEqual(40m, order.Price);
        Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order.OrdStatus);
    }

    [TestMethod]
    public void TestOrderIdIsInitialised()
    {
        var orderSingle = new Fix.Message("8=FIX.4.09=14135=D49=HKGATE56=GEHVWAP115=test34=952=20090728-23:34:516005=ASN=PACISS=V3M0tP0tV1tQ0tU211=121=155=RIO54=138=10040=165=AX44=4010=042");
        var ack = new Fix.Message("8=FIX.4.09=16935=849=GEHVWAP56=HKGATE128=test34=1197=N52=20090728-23:34:526=0.00000011=114=017=120=031=0.00000032=037=138=10039=040=144=0.00000054=155=RIO.AX59=010=254");

        var book = new Fix.OrderBook();

        Assert.AreEqual(0, book.Orders.Count);
        Assert.AreEqual(book.Process(orderSingle), Fix.OrderBookMessageEffect.Modified);
        Assert.AreEqual(1, book.Orders.Count);

        Fix.Order order = book.Orders[0];
        Assert.AreEqual(1, order.Messages.Count);
        Assert.AreEqual("RIO", order.Symbol);
        Assert.AreEqual(40m, order.Price);
        Assert.AreEqual(FIX_5_0SP2.Side.Buy, order.Side);
        Assert.IsNull(order.OrderID);

        Assert.AreEqual(book.Process(ack), Fix.OrderBookMessageEffect.Modified);
        Assert.AreEqual(1, book.Orders.Count);

        order = book.Orders[0];
        Assert.AreEqual(2, order.Messages.Count);
        Assert.AreEqual("RIO", order.Symbol);
        Assert.AreEqual(FIX_5_0SP2.Side.Buy, order.Side);
        Assert.AreEqual(40m, order.Price);
        Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order.OrdStatus);
        Assert.AreEqual("1", order.OrderID);
    }

    [TestMethod]
    public void TestClOrdIdForDifferenceSessionDoesNotAffectOrder()
    {
        var orderSingle = new Fix.Message("8=FIX.4.09=26235=D49=KODIAK56=server34=9550=kgehvwap97=N52=20090727-05:34:0366=068=111=kgehvwap.1.115=AUD21=238=9940=244=39.60000047=A54=155=RIO.AX59=363=0100=ASX203=16000=6001=test6002=6005=ALT=41.58SS=V3M0tP0tV1tQ0tU2ASN=PACI7050=kgehvwap_test10=184");
        var ack = new Fix.Message("8=FIX.4.09=16235=849=cat56=dog34=10957=kgehvwap52=20090727-05:34:036=011=kgehvwap.1.114=017=220=031=032=037=238=9939=340=244=39.60000054=155=RIO65=AX10=054");

        var book = new Fix.OrderBook();

        Assert.AreEqual(0, book.Orders.Count);
        Assert.AreEqual(book.Process(orderSingle), Fix.OrderBookMessageEffect.Modified);
        Assert.AreEqual(1, book.Orders.Count);

        Fix.Order order = book.Orders[0];
        Assert.AreEqual(1, order.Messages.Count);
        Assert.AreEqual("RIO.AX", order.Symbol);
        Assert.AreEqual(39.6m, order.Price);
        Assert.AreEqual(FIX_5_0SP2.Side.Buy, order.Side);
        Assert.IsNull(order.OrderID);

        Assert.AreEqual(book.Process(ack), Fix.OrderBookMessageEffect.Rejected);
        Assert.AreEqual(1, book.Orders.Count);

        order = book.Orders[0];
        Assert.AreEqual(1, order.Messages.Count);
        Assert.AreEqual("RIO.AX", order.Symbol);
        Assert.AreEqual(39.6m, order.Price);
        Assert.AreEqual(FIX_5_0SP2.Side.Buy, order.Side);
        Assert.IsNull(order.OrderID);
    }

    [TestMethod]
    public void TestKodiakNewAckReport()
    {
        var orderSingle = new Fix.Message("8=FIX.4.09=14135=D49=HKGATE56=GEHASF34=3252=20090818-04:15:50115=test6005=ASN=PACISS=V3M0tP0tV1tQ0tU211=321=155=BHP54=138=10040=165=AX44=1510=227");
        var ack = new Fix.Message("8=FIX.4.09=16935=849=GEHASF56=HKGATE128=test34=4397=N52=20090818-04:15:516=0.00000011=314=017=1820=031=0.00000032=037=338=10039=040=144=0.00000054=155=BHP.AX59=010=200");
        var report = new Fix.Message("8=FIX.4.09=20335=849=GEHASF56=HKGATE128=test34=4497=N52=20090818-04:15:516=11.10000011=314=10017=1920=030=ASX31=11.10000032=10037=338=10039=240=144=0.00000054=155=BHP.AX59=060=20090818-04:15:5110=135");

        var book = new Fix.OrderBook();

        Assert.AreEqual(0, book.Orders.Count);
        Assert.AreEqual(book.Process(orderSingle), Fix.OrderBookMessageEffect.Modified);
        Assert.AreEqual(1, book.Orders.Count);

        Fix.Order order = book.Orders[0];
        Assert.AreEqual(1, order.Messages.Count);
        Assert.AreEqual("BHP", order.Symbol);
        Assert.AreEqual(15m, order.Price);
        Assert.AreEqual(FIX_5_0SP2.Side.Buy, order.Side);
        Assert.IsNull(order.OrderID);

        Assert.AreEqual(book.Process(ack), Fix.OrderBookMessageEffect.Modified);
        Assert.AreEqual(1, book.Orders.Count);

        order = book.Orders[0];
        Assert.AreEqual(2, order.Messages.Count);
        Assert.AreEqual("BHP", order.Symbol);
        Assert.AreEqual(FIX_5_0SP2.Side.Buy, order.Side);
        Assert.IsNotNull(order.OrderID);
        Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order.OrdStatus);
        Assert.AreEqual(15m, order.Price);
        Assert.AreEqual(0, order.CumQty);
        Assert.AreEqual(0, order.AvgPx);

        Assert.AreEqual(book.Process(report), Fix.OrderBookMessageEffect.Modified);
        Assert.AreEqual(1, book.Orders.Count);

        order = book.Orders[0];
        Assert.AreEqual(3, order.Messages.Count);
        Assert.AreEqual("BHP", order.Symbol);
        Assert.AreEqual(FIX_5_0SP2.Side.Buy, order.Side);
        Assert.IsNotNull(order.OrderID);
        Assert.AreEqual(FIX_5_0SP2.OrdStatus.Filled, order.OrdStatus);
        Assert.AreEqual(15m, order.Price);
        Assert.AreEqual(100, order.CumQty);
        Assert.AreEqual(11.1m, order.AvgPx);
    }

    [TestMethod]
    public void TestOrderCancelReplace()
    {
        var messages = new List<Fix.Message>
        {
            new Fix.Message("8=FIX.4.29=14035=D49=FIX_CLIENT56=DESK_SERVER34=452=20090824-04:26:40115=test11=121=155=BHP54=160=20090824-04:26:3640=1100=038=500044=23.4510=058"),
            new Fix.Message("8=FIX.4.29=14735=849=DESK_SERVER56=FIX_CLIENT34=452=20090824-04:26:0037=2099011=117=2099020=0150=0151=500039=055=BHP54=138=500032=031=014=06=010=253"),
            new Fix.Message("8=FIX.4.29=15435=G49=FIX_CLIENT56=DESK_SERVER34=552=20090824-04:27:08115=test41=111=221=155=BHP54=160=20090824-04:26:4840=137=20990100=038=600044=23.5010=201"),
            new Fix.Message("8=FIX.4.29=15935=849=DESK_SERVER56=FIX_CLIENT34=552=20090824-04:26:2837=2099011=241=120=039=6150=655=BHP54=138=600044=23.532=031=014=06=0151=600017=113910=038"),
            new Fix.Message("8=FIX.4.29=15935=849=DESK_SERVER56=FIX_CLIENT34=652=20090824-04:26:2837=2099011=241=120=039=5150=555=BHP54=138=600044=23.532=031=014=06=0151=600017=114010=029")
        };
        Assert.AreEqual(5, messages.Count);

        var book = new Fix.OrderBook();

        for (int index = 0; index < messages.Count; ++index)
        {
            Fix.Message message = messages[index];
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);

            Fix.Order? order = null;
            Fix.Order? replacement = null;

            if (book.Orders.Count > 0)
                order = book.Orders[0];

            if (book.Orders.Count > 1)
                replacement = book.Orders[1];

            switch (index)
            {
                case 0: // OrderSingle
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.IsNotNull(order);
                    Assert.IsNull(order?.OrdStatus);
                    Assert.AreEqual(5000, order?.OrderQty);
                    Assert.AreEqual(23.45m, order?.Price);
                    break;

                case 1: // New
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.IsNotNull(order);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order?.OrdStatus);
                    Assert.AreEqual(5000, order?.OrderQty);
                    Assert.AreEqual(23.45m, order?.Price);
                    break;

                case 2: // OrderCancelReplaceRequest
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.IsNotNull(order);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, order?.OrdStatus);
                    Assert.AreEqual(5000, order?.OrderQty);
                    Assert.AreEqual(23.45m, order?.Price);
                    Assert.AreEqual(6000, order?.PendingOrderQty);
                    Assert.AreEqual(23.50m, order?.PendingPrice);
                    break;

                case 3: // Pending Cancel Replace
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.IsNotNull(order);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, order?.OrdStatus);
                    Assert.AreEqual(5000, order?.OrderQty);
                    Assert.AreEqual(23.45m, order?.Price);
                    Assert.AreEqual(6000, order?.PendingOrderQty);
                    Assert.AreEqual(23.50m, order?.PendingPrice);
                    break;

                case 4: // Replaced
                    {
                        Assert.AreEqual(2, book.Orders.Count);
                        Assert.IsNotNull(order);
                        Assert.IsNotNull(replacement);
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, order?.OrdStatus);
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, replacement?.OrdStatus);
                        Assert.AreEqual(5000, order?.OrderQty);
                        Assert.AreEqual(6000, replacement?.OrderQty);
                        Assert.AreEqual(23.50m, replacement?.Price);
                        Assert.IsNull(replacement?.PendingOrderQty);
                        Assert.IsNull(replacement?.PendingPrice);
                        Assert.IsNull(order?.PendingOrderQty);
                        Assert.IsNull(order?.PendingPrice);
                    }
                    break;
            }
        }
    }

    [TestMethod]
    public void TestOrderCancelReplaceAgain()
    {
        var messages = new List<Fix.Message>
        {
            new Fix.Message("8=FIX.4.09=24635=D49=KODIAK56=server34=32850=kgehgap97=N52=20090824-01:55:5266=068=111=kgehgap.1.115=AUD21=238=3540=244=10.00000047=A54=155=RDF.AX59=063=0100=ASX203=16000=6001=test6002=6003=SN=GVW0;LOT6005=SS=V2Y07050=kgehgap_test10=045"),
            new Fix.Message("8=FIX.4.09=16235=849=server56=KODIAK34=32157=kgehgap52=20090824-01:55:546=011=kgehgap.1.114=017=220=031=0.032=037=238=3539=040=244=10.00000054=155=RDF65=AX10=091"),
            new Fix.Message("8=FIX.4.09=20335=G49=KODIAK56=server34=33350=kgehgap97=N52=20090824-01:56:5366=068=111=kgehgap.1.515=AUD21=238=3540=241=kgehgap.1.144=10.70000047=A54=155=RDF.AX59=063=0100=ASX6005=SS=V2Y010=073"),
            new Fix.Message("8=FIX.4.09=16035=849=server56=KODIAK34=32557=kgehgap52=20090824-01:56:536=011=kgehgap.1.514=017=620=031=032=037=738=3539=640=244=10.70000054=155=RDF65=AX10=025"),
            new Fix.Message("8=FIX.4.09=16035=849=server56=KODIAK34=32657=kgehgap52=20090824-01:56:536=011=kgehgap.1.514=017=720=031=032=037=738=3539=540=244=10.70000054=155=RDF65=AX10=026")
        };

        Assert.AreEqual(5, messages.Count);

        var book = new Fix.OrderBook();

        for (int index = 0; index < messages.Count; ++index)
        {
            Fix.Message message = messages[index];
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);

            Fix.Order? order = null;
            Fix.Order? replacement = null;

            if (book.Orders.Count > 0)
                order = book.Orders[0];

            if (book.Orders.Count > 1)
                replacement = book.Orders[1];

            switch (index)
            {
                case 0: // OrderSingle
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.IsNotNull(order);
                    Assert.AreEqual(35, order?.OrderQty);
                    Assert.AreEqual(10m, order?.Price);
                    break;

                case 1: // New
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.IsNotNull(order);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order?.OrdStatus);
                    Assert.AreEqual(35, order?.OrderQty);
                    Assert.AreEqual(10m, order?.Price);
                    break;

                case 2: // OrderCancelReplaceRequest
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.IsNotNull(order);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, order?.OrdStatus);
                    Assert.AreEqual(35, order?.OrderQty);
                    Assert.AreEqual(10m, order?.Price);
                    break;

                case 3: // Pending Cancel Replace
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.IsNotNull(order);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, order?.OrdStatus);
                    Assert.AreEqual(35, order?.OrderQty);
                    Assert.AreEqual(10m, order?.Price);
                    break;

                case 4: // Replaced
                    Assert.AreEqual(2, book.Orders.Count);
                    Assert.IsNotNull(order);
                    Assert.IsNotNull(replacement);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, order?.OrdStatus);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, replacement?.OrdStatus);
                    Assert.AreEqual(35, order?.OrderQty);
                    Assert.AreEqual(35, replacement?.OrderQty);
                    Assert.AreEqual(10.7m, replacement?.Price);
                    break;
            }
        }
    }

    [TestMethod]
    [DeploymentItem("Logs/t_qj.log")]
    public async Task TestQuoteJockeyLog()
    {
        Fix.MessageCollection messages = await Fix.MessageCollection.Parse("t_qj.log");
        Assert.IsNotNull(messages);
        Assert.AreEqual(20, messages.Count);

        var book = new Fix.OrderBook();

        for (int index = 0; index < book.Orders.Count; ++index)
        {
            Assert.AreEqual(book.Process(messages[index]), Fix.OrderBookMessageEffect.Modified);

            switch (index)
            {
                case 0:		// OrderSingle
                    {
                        Assert.AreEqual(1, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.IsNull(order.OrdStatus);
                        Assert.AreEqual(100000, order.OrderQty);
                        Assert.AreEqual(25.42m, order.Price);
                    }
                    break;

                case 1:		// New
                    {
                        Assert.AreEqual(1, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order.OrdStatus);
                        Assert.AreEqual(10000, order.OrderQty);
                        Assert.AreEqual(25.42m, order.Price);
                    }
                    break;

                case 10:	// OrderCancelReplaceRequest
                    {
                        Assert.AreEqual(1, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingCancel, order.OrdStatus);
                        Assert.AreEqual(10000, order.OrderQty);
                        Assert.AreEqual(25.42m, order.Price);
                    }
                    break;

                case 11:	// PendingCancelReplace
                    {
                        Assert.AreEqual(1, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.AreEqual(10000, order.OrderQty);
                        Assert.AreEqual(25.42m, order.Price);
                    }
                    break;

                case 12:	// Replaced
                    {
                        Assert.AreEqual(2, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.AreEqual(10000, order.OrderQty);
                        Assert.AreEqual(25.42m, order.Price);
                        order = book.Orders[1];
                        Assert.AreEqual(10000, order.OrderQty);
                        Assert.AreEqual(25.46m, order.Price);
                    }
                    break;

                case 14:	// OrderCancelReplaceRequest
                    {
                        Assert.AreEqual(2, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.AreEqual(10000, order.OrderQty);
                    }
                    break;

                case 15:	// PendingCancelReplace
                    {
                        Assert.AreEqual(2, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.AreEqual(10000, order.OrderQty);
                    }
                    break;

                case 16:	// Replaced
                    {
                        Assert.AreEqual(3, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.AreEqual(10000, order.OrderQty);
                        Assert.AreEqual(25.42m, order.Price);
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, order.OrdStatus);
                        order = book.Orders[1];
                        Assert.AreEqual(10000, order.OrderQty);
                        Assert.AreEqual(25.46m, order.Price);
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, order.OrdStatus);
                        order = book.Orders[2];
                        Assert.AreEqual(10000, order.OrderQty);
                        Assert.AreEqual(25.5m, order.Price);
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.PartiallyFilled, order.OrdStatus);
                    }
                    break;

                case 19:	// Filled
                    {
                        Assert.AreEqual(3, book.Orders.Count);
                        Fix.Order order = book.Orders[^1];
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.Filled, order.OrdStatus);
                        Assert.AreEqual(10000, order.OrderQty);
                    }
                    break;

                default:
                    {
                        Assert.AreEqual(3, book.Orders.Count);
                        Fix.Order order = book.Orders[^1];
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.PartiallyFilled, order.OrdStatus);
                        Assert.AreEqual(10000, order.OrderQty);
                    }
                    break;

            }
        }
    }

    [TestMethod]
    public async Task TestCancel()
    {
        var text =
            "{\n" +
            "BeginString    (8) - FIX.4.2\n" +
            "BodyLength    (9) - 216\n" +
            "MsgType   (35) - D\n" +
            "SenderCompID   (49) - DEVGATE\n" +
            "TargetCompID   (56) - GEHQJ\n" +
            "SenderSubID   (50) - smart\n" +
            "TargetSubID   (57) - smart\n" +
            "OnBehalfOfCompID  (115) - \n" +
            "MsgSeqNum   (34) - 1010\n" +
            "SendingTime   (52) - 20110609-01:20:04\n" +
            "ClOrdID   (11) - 40-12\n" +
            "OrderQty   (38) - 1000\n" +
            "ExDestination  (100) - TOK\n" +
            "HandlInst   (21) - 1 - Automated execution order, private, no Broker intervention\n" +
            "Price   (44) - 15000.000000\n" +
            "OrdType   (40) - 2 - Limit\n" +
            "TimeInForce   (59) - 0 - Day\n" +
            "Side   (54) - 1 - Buy\n" +
            "SymbolSfx   (65) - OS\n" +
            "Symbol   (55) - 7974\n" +
            "Rule80A   (47) - A - Agency single order\n" +
            "ClientID  (109) - GT40DcSMRT\n" +
            "7050 (7050) - GEHMCFIXGATEINAP\n" +
            "TransactTime   (60) - 20110609-01:20:04\n" +
            "CheckSum   (10) - 020\n" +
            "}\n" +
            "{\n" +
            " BeginString    (8) - FIX.4.2\n" +
            "BodyLength    (9) - 157\n" +
            "MsgType   (35) - 8\n" +
            "SenderCompID   (49) - GEHQJ\n" +
            "TargetCompID   (56) - DEVGATE\n" +
            "MsgSeqNum   (34) - 1023\n" +
            "SendingTime   (52) - 20110609-01:20:03\n" +
            "ExecTransType   (20) - 0 - New\n" +
            "OrdStatus   (39) - 0 - New\n" +
            "ClOrdID   (11) - 40-12\n" +
            "OrderID   (37) - DEVGATE-GEHQJ-40-12\n" +
            "ExecID   (17) - 21\n" +
            "ExecType  (150) - 0 - New\n" +
            "LeavesQty  (151) - 1000\n" +
            "Symbol   (55) - 7974\n" +
            "Side   (54) - 1 - Buy\n" +
            "OrderQty   (38) - 1000\n" +
            "LastShares   (32) - 0\n" +
            "LastPx   (31) - 0\n" +
            "CumQty   (14) - 0\n" +
            "AvgPx    (6) - 0\n" +
            "CheckSum   (10) - 101\n" +
            "}\n" +
            "{\n" +
            "BeginString    (8) - FIX.4.2\n" +
            "BodyLength    (9) - 158\n" +
            "MsgType   (35) - F\n" +
            "SenderCompID   (49) - DEVGATE\n" +
            "TargetCompID   (56) - GEHQJ\n" +
            "SenderSubID   (50) - smart\n" +
            "TargetSubID   (57) - smart\n" +
            "MsgSeqNum   (34) - 1011\n" +
            "SendingTime   (52) - 20110609-01:20:27\n" +
            "OrigClOrdID   (41) - 40-12\n" +
            "ClOrdID   (11) - 40-13\n" +
            "SymbolSfx   (65) - OS\n" +
            "Symbol   (55) - 7974\n" +
            "Side   (54) - 1 - Buy\n" +
            "ClientID  (109) - GT40DcSMRT\n" +
            "TransactTime   (60) - 20110609-01:20:27\n" +
            "OrderQty   (38) - 1000\n" +
            "CheckSum   (10) - 196\n" +
            "}\n" +
            "{\n" +
            "BeginString    (8) - FIX.4.2\n" +
            "BodyLength    (9) - 163\n" +
            "MsgType   (35) - 8\n" +
            "SenderCompID   (49) - GEHQJ\n" +
            "TargetCompID   (56) - DEVGATE\n" +
            "MsgSeqNum   (34) - 1025\n" +
            "SendingTime   (52) - 20110609-01:20:27\n" +
            "ExecTransType   (20) - 0 - New\n" +
            "OrdStatus   (39) - 6 - Pending Cancel Replace\n" +
            "ExecID   (17) - 22\n" +
            "OrderID   (37) - DEVGATE-GEHQJ-40-12\n" +
            "ClOrdID   (11) - 40-13\n" +
            "ExecType  (150) - 6 - Pending Cancel Replace\n" +
            "LeavesQty  (151) - 0\n" +
            "OrigClOrdID   (41) - 40-12\n" +
            "Symbol   (55) - 7974\n" +
            "Side   (54) - 1 - Buy\n" +
            "OrderQty   (38) - 1000\n" +
            "CumQty   (14) - 0\n" +
            "AvgPx    (6) - 0\n" +
            "LastShares   (32) - 0\n" +
            "LastPx   (31) - 0\n" +
            "CheckSum   (10) - 126\n" +
            "}\n" +
            "{\n" +
            "BeginString    (8) - FIX.4.2\n" +
            "BodyLength    (9) - 166\n" +
            "MsgType   (35) - 8\n" +
            "SenderCompID   (49) - GEHQJ\n" +
            "TargetCompID   (56) - DEVGATE\n" +
            "MsgSeqNum   (34) - 1026\n" +
            "SendingTime   (52) - 20110609-01:20:27\n" +
            "ExecTransType   (20) - 0 - New\n" +
            "OrdStatus   (39) - 4 - Cancelled\n" +
            "ExecID   (17) - 23\n" +
            "OrderID   (37) - DEVGATE-GEHQJ-40-12\n" +
            "ClOrdID   (11) - 40-13\n" +
            "ExecType  (150) - 4 - Cancelled\n" +
            "LeavesQty  (151) - 1000\n" +
            "OrigClOrdID   (41) - 40-12\n" +
            "Symbol   (55) - 7974\n" +
            "Side   (54) - 1 - Buy\n" +
            "OrderQty   (38) - 1000\n" +
            "CumQty   (14) - 0\n" +
            "AvgPx    (6) - 0\n" +
            "LastShares   (32) - 0\n" +
            "LastPx   (31) - 0\n" +
            "CheckSum   (10) - 016"
        ;

        Fix.MessageCollection messages = new();

        await foreach (var message in Fix.Parser.Parse(new MemoryStream(Encoding.ASCII.GetBytes(text))))
        {
            messages.Add(message);
        }

        Assert.AreEqual(5, messages.Count);

        var book = new Fix.OrderBook();

        foreach (Fix.Message message in messages)
        {
            Assert.IsNotNull(message);
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);
        }

        Fix.Order order = book.Orders[0];
        Assert.AreEqual(order.OrdStatus, FIX_5_0SP2.OrdStatus.Canceled);
    }

    [TestMethod]
    public void TestNewReplaceCancel()
    {
        var messages = new List<Fix.Message>
        {
            new Fix.Message("8=FIX.4.29=13235=D49=FIX_CLIENT56=QJ34=1052=20110724-03:15:0811=4821=155=BHP.AX54=160=20110724-03:15:0840=2100=ASX38=10044=43.359=010=049"),
            new Fix.Message("8=FIX.4.29=15635=849=QJ56=FIX_CLIENT34=1152=20110724-03:15:0920=039=011=4837=FIX_CLIENT-QJ-4817=4150=0151=10055=BHP.AX54=138=10044=43.332=031=014=06=010=001"),
            new Fix.Message("8=FIX.4.29=15035=G49=FIX_CLIENT56=QJ34=1152=20110724-03:15:1641=4811=4921=155=BHP.AX54=160=20110724-03:15:1140=137=FIX_CLIENT-QJ-48100=ASX38=10059=010=221"),
            new Fix.Message("8=FIX.4.29=16235=849=QJ56=FIX_CLIENT34=1252=20110724-03:15:1620=039=611=4937=FIX_CLIENT-QJ-4817=5150=6151=10041=4855=BHP.AX54=138=10044=43.332=031=014=06=010=026"),
            new Fix.Message("8=FIX.4.29=15235=849=QJ56=FIX_CLIENT34=1352=20110724-03:15:1620=039=511=4837=FIX_CLIENT-QJ-4917=6150=5151=041=4855=BHP.AX54=138=10032=031=014=06=010=074"),
            new Fix.Message("8=FIX.4.29=14635=849=QJ56=FIX_CLIENT34=1452=20110724-03:15:1620=039=411=4937=FIX_CLIENT-QJ-4917=7150=4151=055=BHP.AX54=138=10032=031=014=06=010=063")
        };
        Assert.AreEqual(6, messages.Count);

        var book = new Fix.OrderBook();

        for (int index = 0; index < messages.Count; ++index)
        {
            Fix.Message message = messages[index];
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);

            switch (index)
            {
                case 0: // OrderSingle
                    {
                        Assert.AreEqual(1, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.IsNull(order.OrdStatus);
                    }
                    break;

                case 1: // New
                    {
                        Assert.AreEqual(1, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order.OrdStatus);
                    }
                    break;

                case 2: // OrderCancelReplaceRequest
                    {
                        Assert.AreEqual(1, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, order.OrdStatus);
                    }
                    break;

                case 3: // Pending Cancel Replace
                    {
                        Assert.AreEqual(1, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, order.OrdStatus);
                    }
                    break;

                case 4: // Replaced
                    {
                        Assert.AreEqual(2, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, order.OrdStatus);
                        order = book.Orders[1];
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order.OrdStatus);
                    }
                    break;

                case 5: // Cancelled
                    {
                        Assert.AreEqual(2, book.Orders.Count);
                        Fix.Order order = book.Orders[0];
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, order.OrdStatus);
                        order = book.Orders[1];
                        Assert.AreEqual(FIX_5_0SP2.OrdStatus.Canceled, order.OrdStatus);
                    }
                    break;
            }
        }
    }

    [TestMethod]
    public void TestNewReplaceRejected()
    {
        var messages = new List<Fix.Message>
        {
            new Fix.Message("8=FIX.4.29=13935=D49=FIX_CLIENT56=QJ34=5252=20120109-03:16:5811=1389521=155=BHP.AX54=160=20120109-03:16:5740=2100=DARK38=10044=3759=347=A10=168"),
            new Fix.Message("8=FIX.4.29=16135=849=QJ56=FIX_CLIENT34=6652=20120109-03:17:0220=039=011=1389537=FIX_CLIENT-QJ-1389517=30150=0151=10055=BHP.AX54=138=10044=3732=031=014=06=010=013"),
            new Fix.Message("8=FIX.4.29=14235=F49=FIX_CLIENT56=QJ34=5352=20120109-03:17:0941=1389511=1389655=BHP.AX54=160=20120109-03:17:0737=FIX_CLIENT-QJ-1389565=AX38=10010=202"),
            new Fix.Message("8=FIX.4.29=17035=849=QJ56=FIX_CLIENT34=6752=20120109-03:17:0920=039=611=1389637=FIX_CLIENT-QJ-1389517=31150=6151=10041=1389555=BHP.AX54=138=10044=3732=031=014=06=010=208"),
            new Fix.Message("8=FIX.4.29=11335=949=QJ56=FIX_CLIENT34=6852=20120109-03:17:1337=FIX_CLIENT-QJ-1389539=841=13895434=111=1389658=asdas10=188")
        };
        Assert.AreEqual(5, messages.Count);

        var book = new Fix.OrderBook();

        for (int index = 0; index < messages.Count; ++index)
        {
            Fix.Message message = messages[index];
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);
            Assert.AreEqual(1, book.Orders.Count);
            Fix.Order order = book.Orders[0];

            switch (index)
            {
                case 0: // OrderSingle
                    Assert.IsNull(order.OrdStatus);
                    break;

                case 1: // New
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order.OrdStatus);
                    break;

                case 2: // OrderCancelRequest
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingCancel, order.OrdStatus);
                    break;

                case 3: // Pending Cancel Replace
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingCancel, order.OrdStatus);
                    break;

                case 4: // Rejected
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order.OrdStatus);
                    break;
            }
        }
    }

    [TestMethod]
    public void TestNewPartialReplaceRejected()
    {
        var messages = new List<Fix.Message>
        {
            new Fix.Message("8=FIX.4.29=14035=D49=FIX_CLIENT56=QJ34=13952=20120109-04:48:4811=1390321=155=BHP.AX54=160=20120109-04:48:4740=2100=DARK38=10044=3759=347=A10=214"),
            new Fix.Message("8=FIX.4.29=16235=849=QJ56=FIX_CLIENT34=15752=20120109-04:48:5020=039=011=1390337=FIX_CLIENT-QJ-1390317=38150=0151=10055=BHP.AX54=138=10044=3732=031=014=06=010=059"),
            new Fix.Message("8=FIX.4.29=17335=849=QJ56=FIX_CLIENT34=15852=20120109-04:48:5520=039=111=1390337=FIX_CLIENT-QJ-1390317=39150=1151=5055=BHP.AX54=138=10044=3732=5031=3714=506=3730=DARK10=187"),
            new Fix.Message("8=FIX.4.29=14335=F49=FIX_CLIENT56=QJ34=14052=20120109-04:49:0241=1390311=1390455=BHP.AX54=160=20120109-04:49:0137=FIX_CLIENT-QJ-1390365=AX38=10010=217"),
            new Fix.Message("8=FIX.4.29=18035=849=QJ56=FIX_CLIENT34=15952=20120109-04:49:0220=039=611=1390437=FIX_CLIENT-QJ-1390317=40150=6151=5041=1390355=BHP.AX54=138=10044=3732=031=014=506=3730=DARK10=234"),
            new Fix.Message("8=FIX.4.29=12035=949=QJ56=FIX_CLIENT34=16052=20120109-04:49:1037=FIX_CLIENT-QJ-1390339=841=13903434=111=1390458=sadsadasdas10=056")
        };
        Assert.AreEqual(6, messages.Count);

        var book = new Fix.OrderBook();

        for (int index = 0; index < messages.Count; ++index)
        {
            Fix.Message message = messages[index];
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);
            Assert.AreEqual(1, book.Orders.Count);
            Fix.Order order = book.Orders[0];

            switch (index)
            {
                case 0: // OrderSingle
                    Assert.IsNull(order.OrdStatus);
                    break;

                case 1: // New
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order.OrdStatus);
                    break;

                case 2: // ExecutionReport
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PartiallyFilled, order.OrdStatus);
                    break;

                case 3: // OrderCancelRequest
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingCancel, order.OrdStatus);
                    break;

                case 4: // Pending Cancel Replace
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingCancel, order.OrdStatus);
                    break;

                case 5: // Rejected
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PartiallyFilled, order.OrdStatus);
                    break;
            }
        }
    }

    [TestMethod]
    public void TestNewReplaceRejectedAgain()
    {
        var messages = new List<Fix.Message>
        {
            new Fix.Message("8=FIX.4.29=14035=D49=FIX_CLIENT56=QJ34=15552=20120109-05:26:0311=1391321=155=BHP.AX54=160=20120109-05:26:0240=2100=DARK38=10044=3759=347=A10=189"),
            new Fix.Message("8=FIX.4.29=16235=849=QJ56=FIX_CLIENT34=18452=20120109-05:26:0820=039=011=1391337=FIX_CLIENT-QJ-1391317=44150=0151=10055=BHP.AX54=138=10044=3732=031=014=06=010=058"),
            new Fix.Message("8=FIX.4.29=22735=G49=FIX_CLIENT56=QJ34=15652=20120109-05:26:2390=3116=200129=300145=2041=1391311=1391421=155=BHP.AX54=160=20120109-05:26:1440=237=FIX_CLIENT-QJ-1391318=M110=51100=DARK65=AX38=20044=1259=347=A58=sadas10=026"),
            new Fix.Message("8=FIX.4.29=17135=849=QJ56=FIX_CLIENT34=18552=20120109-05:26:2320=039=611=1391437=FIX_CLIENT-QJ-1391317=45150=6151=10041=1391355=BHP.AX54=138=10044=3732=031=014=06=010=234"),
            new Fix.Message("8=FIX.4.29=11335=949=QJ56=FIX_CLIENT34=18652=20120109-05:26:3037=FIX_CLIENT-QJ-1391339=841=13913434=211=1391458=blah10=095")
        };
        Assert.AreEqual(5, messages.Count);

        var book = new Fix.OrderBook();

        for (int index = 0; index < messages.Count; ++index)
        {
            Fix.Message message = messages[index];
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);
            Assert.AreEqual(1, book.Orders.Count);
            Fix.Order order = book.Orders[0];

            switch (index)
            {
                case 0: // OrderSingle
                    Assert.IsNull(order.OrdStatus);
                    break;

                case 1: // New
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order.OrdStatus);
                    break;

                case 2: // OrderCancelReplaceRequest
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, order.OrdStatus);
                    break;

                case 3: // Pending Cancel Replace
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, order.OrdStatus);
                    break;

                case 4: // Rejected
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order.OrdStatus);
                    break;
            }
        }
    }

    [TestMethod]
    [DeploymentItem("Logs/t_fix_4_0_amend.log")]
    public async Task TestFix40Amend()
    {
        Fix.MessageCollection messages = await Fix.MessageCollection.Parse("t_fix_4_0_amend.log");
        Assert.IsNotNull(messages);
        Assert.AreEqual(6, messages.Count);

        var book = new Fix.OrderBook();

        foreach (Fix.Message message in messages)
        {
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);
        }

        Assert.AreEqual(2, book.Orders.Count);
        Fix.Order order1 = book.Orders[0];
        Fix.Order order2 = book.Orders[1];

        Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, order1.OrdStatus);
        Assert.AreEqual(FIX_5_0SP2.OrdStatus.PartiallyFilled, order2.OrdStatus);
    }

    [TestMethod]
    [DeploymentItem("Logs/t_cancel_leavesqty.log")]
    public async Task TestCancelLeavesQty()
    {
        Fix.MessageCollection messages = await Fix.MessageCollection.Parse("t_cancel_leavesqty.log");
        Assert.IsNotNull(messages);
        Assert.AreEqual(5, messages.Count);

        var book = new Fix.OrderBook();

        foreach (Fix.Message message in messages)
        {
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);
        }

        Assert.AreEqual(1, book.Orders.Count);
        Fix.Order order = book.Orders[0];

        Assert.AreEqual(5, order.Messages.Count);
        Assert.AreEqual(0, order.LeavesQty);
    }

    [TestMethod]
    [DeploymentItem("Logs/t_new_amend.log")]
    public async Task TestAmend()
    {
        Fix.MessageCollection messages = await Fix.MessageCollection.Parse("t_new_amend.log");
        Assert.IsNotNull(messages);
        Assert.AreEqual(5, messages.Count);

        var book = new Fix.OrderBook();

        foreach (Fix.Message message in messages)
        {
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);
        }

        Assert.AreEqual(2, book.Orders.Count);
        Fix.Order order = book.Orders[0];
        Fix.Order replacement = book.Orders[1];

        Assert.AreEqual(5, order.Messages.Count);
        Assert.AreEqual(6, replacement.Messages.Count);
        Assert.AreEqual(0, order.LeavesQty);
        Assert.AreEqual(2286, replacement.LeavesQty);
    }

    [TestMethod]
    [DeploymentItem("Logs/t_upward_amend_filled.log")]
    public async Task TestAmendUp()
    {
        Fix.MessageCollection messages = await Fix.MessageCollection.Parse("t_upward_amend_filled.log");
        Assert.IsNotNull(messages);
        Assert.AreEqual(7, messages.Count);

        var book = new Fix.OrderBook();

        for (int index = 0; index < messages.Count; ++index)
        {
            Fix.Message message = messages[index];
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);
            Fix.Order original = book.Orders[0];
            Fix.Order? replacement = null;

            if (book.Orders.Count > 1)
            {
                replacement = book.Orders[1];
            }

            switch (index)
            {
                case 0:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.IsNull(original.OrdStatus);
                    break;

                case 1:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, original.OrdStatus);
                    break;

                case 2:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Filled, original.OrdStatus);
                    break;

                case 3:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, original.OrdStatus);
                    break;

                case 4:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, original.OrdStatus);
                    break;

                case 5:
                    Assert.AreEqual(2, book.Orders.Count);
                    Assert.IsNotNull(replacement);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, original.OrdStatus);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PartiallyFilled, replacement?.OrdStatus);
                    break;

                case 6:
                    Assert.AreEqual(2, book.Orders.Count);
                    Assert.IsNotNull(replacement);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, original.OrdStatus);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Filled, replacement?.OrdStatus);
                    break;
            }
        }
    }

    [TestMethod]
    [DeploymentItem("Logs/t_upward_amend_filled_to_partial.log")]
    public async Task TestUpwardAmendFilledToPartial()
    {
        Fix.MessageCollection messages = await Fix.MessageCollection.Parse("t_upward_amend_filled_to_partial.log");
        Assert.IsNotNull(messages);
        Assert.AreEqual(7, messages.Count);

        var book = new Fix.OrderBook();

        for (int index = 0; index < messages.Count; ++index)
        {
            Fix.Message message = messages[index];
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);
            Fix.Order original = book.Orders[0];
            Fix.Order? replacement = null;

            if (book.Orders.Count > 1)
            {
                replacement = book.Orders[1];
            }

            switch (index)
            {
                case 0:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.IsNull(original.OrdStatus);
                    break;

                case 1:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, original.OrdStatus);
                    break;

                case 2:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PartiallyFilled, original.OrdStatus);
                    break;

                case 3:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Filled, original.OrdStatus);
                    break;

                case 4:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, original.OrdStatus);
                    break;

                case 5:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, original.OrdStatus);
                    break;

                case 6:
                    Assert.AreEqual(2, book.Orders.Count);
                    Assert.IsNotNull(replacement);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, original.OrdStatus);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PartiallyFilled, replacement?.OrdStatus);
                    break;
            }
        }
    }

    [TestMethod]
    [DeploymentItem("Logs/t_fill_while_cancel_pending.log")]
    public async Task TestFillWhileCancelPending()
    {
        Fix.MessageCollection messages = await Fix.MessageCollection.Parse("t_fill_while_cancel_pending.log");
        Assert.IsNotNull(messages);
        Assert.AreEqual(6, messages.Count);

        var book = new Fix.OrderBook();

        for (int index = 0; index < messages.Count; ++index)
        {
            Fix.Message message = messages[index];
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);
            Fix.Order order = book.Orders[0];

            switch (index)
            {
                case 0:
                    Assert.IsNull(order.OrdStatus);
                    break;

                case 1:
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, order.OrdStatus);
                    break;

                case 2:
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingCancel, order.OrdStatus);
                    break;

                case 3:
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingCancel, order.OrdStatus);
                    break;

                case 4:
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingCancel, order.OrdStatus);
                    Assert.AreEqual(500, order.LeavesQty);
                    Assert.AreEqual(500, order.CumQty);
                    break;

                case 5:
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Canceled, order.OrdStatus);
                    Assert.AreEqual(0, order.LeavesQty);
                    Assert.AreEqual(500, order.CumQty);
                    break;
            }
        }
    }

    [TestMethod]
    [DeploymentItem("Logs/t_fill_while_amend_pending.log")]
    public async Task TestFillWhileAmendPending()
    {
        Fix.MessageCollection messages = await Fix.MessageCollection.Parse("t_fill_while_amend_pending.log");
        Assert.IsNotNull(messages);
        Assert.AreEqual(6, messages.Count);

        var book = new Fix.OrderBook();

        for (int index = 0; index < messages.Count; ++index)
        {
            Fix.Message message = messages[index];
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);
            Fix.Order original = book.Orders[0];
            Fix.Order? replacement = null;

            if (book.Orders.Count > 1)
            {
                replacement = book.Orders[1];
            }

            switch (index)
            {
                case 0:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.IsNull(original.OrdStatus);
                    break;

                case 1:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, original.OrdStatus);
                    break;

                case 2:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, original.OrdStatus);
                    break;

                case 3:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, original.OrdStatus);
                    break;

                case 4:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, original.OrdStatus);
                    Assert.AreEqual(500, original.LeavesQty);
                    Assert.AreEqual(500, original.CumQty);
                    break;

                case 5:
                    Assert.AreEqual(2, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, original.OrdStatus);
                    Assert.AreEqual(0, original.LeavesQty);
                    Assert.AreEqual(500, original.CumQty);
                    Assert.IsNotNull(replacement);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PartiallyFilled, replacement?.OrdStatus);
                    Assert.AreEqual(1500, replacement?.LeavesQty);
                    Assert.AreEqual(500, replacement?.CumQty);
                    break;
            }
        }
    }

    [TestMethod]
    [DeploymentItem("Logs/t_amend_cancel.log")]
    public async Task TestAmendCancel()
    {
        Fix.MessageCollection messages = await Fix.MessageCollection.Parse("t_amend_cancel.log");
        Assert.IsNotNull(messages);
        Assert.AreEqual(8, messages.Count);

        var book = new Fix.OrderBook();

        for (int index = 0; index < messages.Count; ++index)
        {
            Fix.Message message = messages[index];
            Assert.AreEqual(book.Process(message), Fix.OrderBookMessageEffect.Modified);
            Fix.Order original = book.Orders[0];
            Fix.Order? replacement = null;

            if (book.Orders.Count > 1)
            {
                replacement = book.Orders[1];
            }

            switch (index)
            {
                case 0:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.IsNull(original.OrdStatus);
                    Assert.AreEqual("BDE122730", original.ClOrdID);
                    break;

                case 1:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, original.OrdStatus);
                    Assert.AreEqual("BDE122730", original.ClOrdID);
                    break;

                case 2:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, original.OrdStatus);
                    Assert.AreEqual("BDE122730", original.ClOrdID);
                    break;

                case 3:
                    Assert.AreEqual(1, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingReplace, original.OrdStatus);
                    Assert.AreEqual("BDE122730", original.ClOrdID);
                    break;

                case 4:
                    Assert.AreEqual(2, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, original.OrdStatus);
                    Assert.AreEqual("BDE122730", original.ClOrdID);
                    Assert.IsNotNull(replacement);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.New, replacement?.OrdStatus);
                    Assert.AreEqual("BDE122792", replacement?.ClOrdID);
                    break;

                case 5:
                    Assert.AreEqual(2, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, original.OrdStatus);
                    Assert.AreEqual("BDE122730", original.ClOrdID);
                    Assert.IsNotNull(replacement);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingCancel, replacement?.OrdStatus);
                    Assert.AreEqual("BDE122792", replacement?.ClOrdID);
                    break;

                case 6:
                    Assert.AreEqual(2, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, original.OrdStatus);
                    Assert.AreEqual("BDE122730", original.ClOrdID);
                    Assert.IsNotNull(replacement);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.PendingCancel, replacement?.OrdStatus);
                    Assert.AreEqual("BDE122792", replacement?.ClOrdID);
                    break;

                case 7:
                    Assert.AreEqual(2, book.Orders.Count);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Replaced, original.OrdStatus);
                    Assert.AreEqual("BDE122730", original.ClOrdID);
                    Assert.IsNotNull(replacement);
                    Assert.AreEqual(FIX_5_0SP2.OrdStatus.Canceled, replacement?.OrdStatus);
                    Assert.AreEqual("BDE122792", replacement?.ClOrdID);
                    break;
            }
        }
    }
}
