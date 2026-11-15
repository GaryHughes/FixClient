using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Fix.Dictionary;

namespace FixTests;

[TestClass]
public class SessionStreamTests
{
    sealed class WriteCountingStream : MemoryStream
    {
        public int Writes { get; private set; }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ++Writes;
            base.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            ++Writes;
            base.Write(buffer);
        }
    }

    sealed class FailingStream : MemoryStream
    {
        public override void Write(byte[] buffer, int offset, int count) => throw new IOException("The connection has gone");

        public override void Write(ReadOnlySpan<byte> buffer) => throw new IOException("The connection has gone");
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void TestSendWritesEachMessageToTheStreamOnce(bool fragmentMessages)
    {
        using var stream = new WriteCountingStream();
        using var session = new Fix.Session
        {
            SenderCompId = "INITIATOR",
            TargetCompId = "ACCEPTOR",
            FragmentMessages = fragmentMessages,
            Stream = stream
        };

        session.Send(new Fix.Message(FIX_5_0SP2.Messages.NewOrderSingle));

        Assert.AreEqual(1, stream.Writes);
        Assert.IsTrue(stream.Length > 0);
    }

    [TestMethod]
    public void TestSendToAFailedConnectionDisconnects()
    {
        using var stream = new FailingStream();
        using var session = new Fix.Session
        {
            SenderCompId = "INITIATOR",
            TargetCompId = "ACCEPTOR",
            Stream = stream
        };

        session.Send(new Fix.Message(FIX_5_0SP2.Messages.NewOrderSingle));

        Assert.AreEqual(Fix.State.Disconnected, session.State);
    }

    [TestMethod]
    public void TestDisposeClosesTheStreamWithoutRaisingEvents()
    {
        var stream = new MemoryStream();
        var session = new Fix.Session
        {
            SenderCompId = "INITIATOR",
            TargetCompId = "ACCEPTOR",
            Stream = stream
        };

        int stateChanges = 0;
        session.StateChanged += (sender, ev) => ++stateChanges;

        session.Dispose();

        Assert.IsFalse(stream.CanRead);
        Assert.AreEqual(0, stateChanges);
        Assert.IsFalse(session.Connected);

        // A second Dispose does nothing.
        session.Dispose();
    }
}
