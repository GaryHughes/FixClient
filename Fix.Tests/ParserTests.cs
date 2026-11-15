using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FixTests;

[TestClass]
public class ParserTests
{
    public async Task<Fix.MessageCollection> ParseMessageCollection<Parser>(string filename) where Parser : Fix.LogParser, new()
    {
        var result = new Fix.MessageCollection();
        var url = new Uri($"file://{Path.GetFullPath(filename)}");
        using var stream = new FileStream(url.LocalPath, FileMode.Open);
        var parser = new Parser();
        await foreach (var message in parser.Parse(stream))
        {
            if (message is null)
            {
                break;
            }
            result.Add(message);
        }
        return result;
    }

    static async Task<List<Fix.Message>> ParseText(string text)
    {
        using var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(text));
        var messages = new List<Fix.Message>();
        await foreach (var message in Fix.Parser.Parse(stream))
        {
            messages.Add(message);
        }
        return messages;
    }

    [TestMethod]
    public async Task TestParserStateDoesNotLeakBetweenParses()
    {
        // ExecType does not exist in FIX 4.0. If the FIX 4.0 version seen in the first log leaked
        // into the second parse the ExecType description would not be recognised and stripped.
        const string fix40 = "{\n    BeginString    (8) - FIX.4.0\n        MsgType   (35) - 0\n}\n";
        const string noVersion = "{\n        MsgType   (35) - 8\n       ExecType  (150) - 0 - New\n}\n";

        var first = await ParseText(fix40);
        Assert.HasCount(1, first);

        var second = await ParseText(noVersion);
        Assert.HasCount(1, second);
        Assert.AreEqual("0", second[0].Fields.Find(150)?.Value);
    }

    [TestMethod]
    public async Task TestParseRejectsNonFileUri()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await foreach (var message in Fix.Parser.Parse(new Uri("http://example.com/log.txt")))
            {
            }
        });

        Assert.AreEqual("uri", exception.ParamName);
        Assert.IsTrue(exception.Message.Contains("'http'"), exception.Message);
    }

    [TestMethod]
    [DeploymentItem("Logs/t_kls.log")]
    public async Task TestKlsParser()
    {
        var messages = await ParseMessageCollection<Fix.Parsers.KlsLogParser>("t_kls.log");
        Assert.AreEqual(95, messages.Count);
    }

    [TestMethod]
    [DeploymentItem("Logs/t_atlas.log")]
    public async Task TestAtlasParser()
    {
        var messages = await ParseMessageCollection<Fix.Parsers.GenericLogParser>("t_atlas.log");
        Assert.AreEqual(473, messages.Count);
    }

    [TestMethod]
    [DeploymentItem("Logs/t_gate_driver.log")]
    public async Task TestGateDriverParser()
    {
        var messages = await ParseMessageCollection<Fix.Parsers.FormattedLogParser>("t_gate_driver.log");
        Assert.AreEqual(43, messages.Count);
    }

    [TestMethod]
    [DeploymentItem("Logs/t_gate_ci.log")]
    public async Task TestGateCiParser()
    {
        var messages = await ParseMessageCollection<Fix.Parsers.FormattedLogParser>("t_gate_ci.log");
        Assert.AreEqual(58, messages.Count);
    }

    [TestMethod]
    [DeploymentItem("Logs/t_gate_raw_driver.log")]
    public async Task TestParseGateRawDriverLog()
    {
        var messages = await ParseMessageCollection<Fix.Parsers.GenericLogParser>("t_gate_raw_driver.log");
        Assert.AreEqual(1466, messages.Count);
    }

    [TestMethod]
    [DeploymentItem("Logs/t_gate_raw_ci.log")]
    public async Task TestParseGateRawCiLog()
    {
        var messages = await ParseMessageCollection<Fix.Parsers.GenericLogParser>("t_gate_raw_ci.log");
        Assert.AreEqual(4909, messages.Count);
    }
    /*
    [TestMethod]
    public void TestMessageParserStripsValueDescription()
    {
        const string text = @"2014-08-26 10:20:56.0006|DEBUG|AwesomO.Execution.FixClient|Outgoing
                            {
                                        BeginString    (8) - FIXT.1.1
                                        BodyLength    (9) - 
                                            MsgType   (35) - 8 - ExecutionReport
                                    SenderCompID   (49) - QJ
                                    TargetCompID   (56) - FIX_CLIENT
                                        MsgSeqNum   (34) - 5019
                                        SendingTime   (52) - 20140826-00:20:55.998
                                        OrdStatus   (39) - 0 - New
                                            ClOrdID   (11) - 20
                                            OrderID   (37) - FIX_CLIENT-QJ-20
                                            ExecID   (17) - 1
                                        ExecType  (150) - 0 - New
                                        LeavesQty  (151) - 10000
                                        SecurityID   (48) - RIO.AX
                                SecurityIDSource   (22) - 5 - RIC code
                                            Side   (54) - 1 - Buy
                                        OrderQty   (38) - 10000
                                            Price   (44) - 97.08
                                            LastQty   (32) - 0
                                            LastPx   (31) - 0
                                            CumQty   (14) - 0
                                            AvgPx    (6) - 0
                                            OrdType   (40) - 2 - Limit
                            }";
        Fix.Message? message = Fix.Message.Parse(text);
        Assert.IsNotNull(message);
        Fix.Field? msgType = message?.Fields.Find(FIX_5_0SP2.Fields.MsgType);
        Assert.IsNotNull(msgType);
        Assert.AreEqual(FIX_5_0SP2.Messages.ExecutionReport.MsgType, msgType?.Value);

    }
    */
}

