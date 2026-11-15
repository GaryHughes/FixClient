using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace FixTests;

[TestClass]
public class PersistentSessionFileTests
{
    [TestMethod]
    public void TestReadWithNoFileName()
    {
        using var session = new Fix.PersistentSession();
        Assert.Throws<InvalidOperationException>(session.Read);
    }

    static string CopyTestData(string name)
    {
        string directory = Path.Combine(Path.GetTempPath(), "FixTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, name);
        File.Copy(Path.Combine(AppContext.BaseDirectory, "TestData", name), path);
        return path;
    }

    // A session file written by FixClient, which also stores the properties of its own Session
    // subclass. Fix.PersistentSession reads the properties it knows and ignores the rest.
    [TestMethod]
    public void TestReadSessionFileWrittenByFixClient()
    {
        string path = CopyTestData("INITIATOR-ACCEPTOR.session");
        try
        {
            using var session = new Fix.PersistentSession { FileName = path, PersistMessages = false };
            session.Read();

            Assert.AreEqual(Fix.Behaviour.Initiator, session.OrderBehaviour);
            Assert.AreEqual(Fix.Behaviour.Initiator, session.LogonBehaviour);
            Assert.AreSame(Fix.Dictionary.Versions.FIXT_1_1, session.BeginString);
            Assert.AreSame(Fix.Dictionary.Versions.FIX_5_0SP2, session.DefaultApplVerId);
            Assert.AreEqual("INITIATOR", session.SenderCompId);
            Assert.AreEqual("ACCEPTOR", session.TargetCompId);
            Assert.AreEqual(5, session.IncomingSeqNum);
            Assert.AreEqual(6, session.OutgoingSeqNum);
            Assert.AreEqual("yyyyMMdd-HH:mm:ss.fff", session.OutgoingTimestampFormat);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }

    [TestMethod]
    public void TestWriteThenRead()
    {
        string path = CopyTestData("INITIATOR-ACCEPTOR.session");
        try
        {
            using (var session = new Fix.PersistentSession
            {
                FileName = path,
                PersistMessages = false,
                BeginString = Fix.Dictionary.Versions.FIX_4_4,
                SenderCompId = "SENDER",
                TargetCompId = "TARGET",
                IncomingSeqNum = 42,
                OutgoingSeqNum = 43,
                LogonBehaviour = Fix.Behaviour.Acceptor,
                OutgoingTimestampFormat = Fix.Field.TimestampFormatShort,
                ValidateMessages = false,
            })
            {
                session.Write();
            }

            using var read = new Fix.PersistentSession { FileName = path, PersistMessages = false };
            read.Read();

            Assert.AreSame(Fix.Dictionary.Versions.FIX_4_4, read.BeginString);
            Assert.AreEqual("SENDER", read.SenderCompId);
            Assert.AreEqual("TARGET", read.TargetCompId);
            Assert.AreEqual(42, read.IncomingSeqNum);
            Assert.AreEqual(43, read.OutgoingSeqNum);
            Assert.AreEqual(Fix.Behaviour.Acceptor, read.LogonBehaviour);
            Assert.AreEqual(Fix.Field.TimestampFormatShort, read.OutgoingTimestampFormat);
            Assert.IsFalse(read.ValidateMessages);

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            Assert.AreEqual("FIX.4.4", document.RootElement.GetProperty("BeginString").GetString());
            Assert.AreEqual("Acceptor", document.RootElement.GetProperty("LogonBehaviour").GetString());
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }
}
