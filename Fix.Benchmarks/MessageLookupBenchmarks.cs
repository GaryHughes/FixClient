using BenchmarkDotNet.Attributes;

namespace Fix.Benchmarks;

//
// Versions[beginString].Messages[msgType] is called for every message sent, received, logged or
// displayed. The generated message lists start with the session messages and the order flow
// messages that make up most traffic, so a linear search finds those after only a few
// comparisons. These benchmarks compare a FrozenDictionary lookup with the LINQ search it
// replaced, and with a plain loop, by where the message sits in the list.
//
// Position in the list (the same in FIX.4.4 and FIX.5.0SP2):
//   0  Heartbeat           0
//   8  ExecutionReport     8
//   D  NewOrderSingle      13
//   AE TradeCaptureReport  63
//   ZZ not defined, so a search examines all 93 (FIX.4.4) or 164 (FIX.5.0SP2) messages
//
[MemoryDiagnoser]
public class MessageLookupBenchmarks
{
    [Params("FIX.4.4", "FIX.5.0SP2")]
    public string BeginString { get; set; } = string.Empty;

    [Params("0", "8", "D", "AE", "ZZ")]
    public string MsgType { get; set; } = string.Empty;

    Dictionary.VersionMessageCollection _messages = null!;

    [GlobalSetup]
    public void Setup()
    {
        _messages = Dictionary.Versions[BeginString]?.Messages ?? throw new InvalidOperationException($"Unknown version {BeginString}");
    }

    [Benchmark(Baseline = true)]
    public Dictionary.Message? LinqSearch() => MessageLookup.LinqSearch(_messages, MsgType);

    [Benchmark]
    public Dictionary.Message? LoopSearch() => MessageLookup.LoopSearch(_messages, MsgType);

    [Benchmark]
    public Dictionary.Message? FrozenDictionary() => _messages[MsgType];
}

//
// A stream of lookups in roughly the proportions a trading session sees: mostly execution reports
// and orders, some cancels and heartbeats, and the occasional other session message. All of them
// are within the first 17 messages of the list. The order is shuffled with a fixed seed so every run
// sees the same sequence.
//
[MemoryDiagnoser]
public class MessageLookupTrafficBenchmarks
{
    const int Lookups = 100;

    static readonly string[] Traffic = CreateTraffic();

    static string[] CreateTraffic()
    {
        (string MsgType, int Count)[] mix =
        [
            ("8", 45), // ExecutionReport
            ("D", 20), // NewOrderSingle
            ("0", 10), // Heartbeat
            ("F", 8),  // OrderCancelRequest
            ("G", 7),  // OrderCancelReplaceRequest
            ("9", 3),  // OrderCancelReject
            ("1", 2),  // TestRequest
            ("2", 1),  // ResendRequest
            ("3", 1),  // Reject
            ("4", 1),  // SequenceReset
            ("5", 1),  // Logout
            ("A", 1),  // Logon
        ];
        var traffic = mix.SelectMany(entry => Enumerable.Repeat(entry.MsgType, entry.Count)).ToArray();
        new Random(1).Shuffle(traffic);
        return traffic.Length == Lookups ? traffic : throw new InvalidOperationException("The traffic mix must total 100");
    }

    [Params("FIX.4.4", "FIX.5.0SP2")]
    public string BeginString { get; set; } = string.Empty;

    Dictionary.VersionMessageCollection _messages = null!;

    [GlobalSetup]
    public void Setup()
    {
        _messages = Dictionary.Versions[BeginString]?.Messages ?? throw new InvalidOperationException($"Unknown version {BeginString}");
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = Lookups)]
    public int LinqSearch()
    {
        int found = 0;
        foreach (var msgType in Traffic)
        {
            if (MessageLookup.LinqSearch(_messages, msgType) is not null)
                ++found;
        }
        return found;
    }

    [Benchmark(OperationsPerInvoke = Lookups)]
    public int LoopSearch()
    {
        int found = 0;
        foreach (var msgType in Traffic)
        {
            if (MessageLookup.LoopSearch(_messages, msgType) is not null)
                ++found;
        }
        return found;
    }

    [Benchmark(OperationsPerInvoke = Lookups)]
    public int FrozenDictionary()
    {
        int found = 0;
        foreach (var msgType in Traffic)
        {
            if (_messages[msgType] is not null)
                ++found;
        }
        return found;
    }
}

static class MessageLookup
{
    // The implementation VersionMessageCollection used before the FrozenDictionary.
    public static Dictionary.Message? LinqSearch(Dictionary.VersionMessageCollection messages, string msgType)
    {
        return (from message in messages.Messages
                where message.MsgType == msgType
                select message).FirstOrDefault();
    }

    public static Dictionary.Message? LoopSearch(Dictionary.VersionMessageCollection messages, string msgType)
    {
        foreach (var message in messages.Messages)
        {
            if (message.MsgType == msgType)
                return message;
        }
        return null;
    }
}
