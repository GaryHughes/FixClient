using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Fix.Dictionary;

namespace FixTests;

[TestClass]
public class PersistentSessionTests : SessionTestsBase<Fix.PersistentSession>
{
    #region Setup

    const string SenderCompId = "INITIATOR";
    const string TargetCompId = "ACCEPTOR";

    readonly object _serialiser = new();

    [TestInitialize]
    public void TestInitialize()
    {
        Monitor.Enter(_serialiser);
        Initialize();
        InitialiseInitiator();
        InitialiseAcceptor();
    }

    void InitialiseInitiator()
    {
        Initiator.SenderCompId = SenderCompId;
        Initiator.TargetCompId = TargetCompId;
        Initiator.TestRequestDelay = 0;
        Initiator.NextExpectedMsgSeqNum = true;
        Initiator.FileName = Path.Combine(Path.GetTempPath(), $"{Initiator.SenderCompId}-{Initiator.TargetCompId}.session");
        if (File.Exists(Initiator.FileName))
        {
            File.Delete(Initiator.FileName);
        }
    }

    void InitialiseAcceptor()
    {
        Acceptor.SenderCompId = TargetCompId;
        Acceptor.TargetCompId = SenderCompId;
        Acceptor.TestRequestDelay = 0;
        Acceptor.NextExpectedMsgSeqNum = true;
        Acceptor.FileName = Path.Combine(Path.GetTempPath(), $"{Acceptor.SenderCompId}-{Acceptor.TargetCompId}.session");
        if (File.Exists(Acceptor.FileName))
        {
            File.Delete(Acceptor.FileName);
        }
    }

    [TestCleanup]
    public void TestCleanup()
    {
        Cleanup();
        Monitor.Exit(_serialiser);
    }
    #endregion

    [TestMethod]
    public void TestWriteReplacesSessionFileAndLeavesNoTemporaryFile()
    {
        string fileName = Initiator.FileName!;

        Initiator.HeartBtInt = 45;
        Initiator.Write();

        Assert.IsTrue(File.Exists(fileName));
        Assert.IsFalse(File.Exists(fileName + ".tmp"));
        Assert.IsTrue(File.ReadAllText(fileName).Contains("\"HeartBtInt\": 45"));

        Initiator.HeartBtInt = 46;
        Initiator.Write();

        Assert.IsFalse(File.Exists(fileName + ".tmp"));
        Assert.IsTrue(File.ReadAllText(fileName).Contains("\"HeartBtInt\": 46"));
    }

    [TestMethod]
    public void TestWriteKeepsPreviousSessionFileWhileItIsLocked()
    {
        string fileName = Initiator.FileName!;

        Initiator.HeartBtInt = 45;
        Initiator.Write();
        string before = File.ReadAllText(fileName);

        var warnings = new List<string>();
        Initiator.Warning += (sender, ev) => warnings.Add(ev.Message);

        Initiator.HeartBtInt = 46;

        using (new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            Initiator.Write();
        }

        Assert.AreEqual(before, File.ReadAllText(fileName));
        Assert.HasCount(1, warnings);
        Assert.AreEqual(Fix.State.Connected, Initiator.State);

        Initiator.Write();

        Assert.IsTrue(File.ReadAllText(fileName).Contains("\"HeartBtInt\": 46"));
        Assert.IsFalse(File.Exists(fileName + ".tmp"));
    }

    [TestMethod]
    public void TestCorrectNextExpectedMsgSeqNumAtInitiatorAndAcceptor()
    {
        Assert.AreEqual(Fix.State.Connected, Acceptor.State);
        Assert.AreEqual(Fix.State.Connected, Initiator.State);

        Acceptor.Open();
        Initiator.Open();

        AcceptorStateChange(Fix.State.LoggingOn);
        InitiatorStateChange(Fix.State.LoggingOn);

        ReceiveAtAcceptor(FIX_5_0SP2.Messages.Logon);
        ReceiveAtInitiator(FIX_5_0SP2.Messages.Logon);

        AcceptorStateChange(Fix.State.LoggedOn);
        InitiatorStateChange(Fix.State.LoggedOn);
    }

    [TestMethod]
    public void TestNextExpectedMsgSeqNumHigherThanExpected()
    {
        Initiator.OutgoingSeqNum = 3640;
        Initiator.IncomingSeqNum = 1086;

        Assert.AreEqual(Fix.State.Connected, Acceptor.State);
        Assert.AreEqual(Fix.State.Connected, Initiator.State);

        Acceptor.Open();
        Initiator.Open();

        AcceptorStateChange(Fix.State.LoggingOn);
        InitiatorStateChange(Fix.State.LoggingOn);

        ReceiveAtInitiator(FIX_5_0SP2.Messages.Logout, new[]
        {
            new Fix.Field(FIX_5_0SP2.Fields.Text, "NextExpectedMsgSeqNum too high, expecting 1 but received 1086")
        });

        AcceptorStateChange(Fix.State.Disconnected);
        InitiatorStateChange(Fix.State.Disconnected);

        Reconnect();
        InitialiseAcceptor();

        InitiatorStateChange(Fix.State.Connected);

        Acceptor.Open();
        Initiator.Open();

        AcceptorStateChange(Fix.State.LoggingOn);
        InitiatorStateChange(Fix.State.LoggingOn);

        ReceiveAtInitiator(FIX_5_0SP2.Messages.Logout, new[]
        {
            new Fix.Field(FIX_5_0SP2.Fields.Text, "NextExpectedMsgSeqNum too high, expecting 1 but received 1086")
        });

        AcceptorStateChange(Fix.State.Disconnected);
        InitiatorStateChange(Fix.State.Disconnected);
    }
}

