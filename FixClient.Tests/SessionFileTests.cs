using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text.Json;

namespace FixClientTests;

//
// The files in TestData were written by FixClient. These tests check that session, filter and custom
// field files are read the same way, and that what FixClient writes reads back the same, so changes
// to how the files are read and written stay compatible with files people already have.
//
[TestClass]
public class SessionFileTests
{
    static readonly int[] OrderFieldFilters = [8, 9, 35, 49, 56, 34, 52, 11, 70, 55, 54, 60, 38, 40, 2433, 10];

    string _directory = string.Empty;

    [TestInitialize]
    public void TestInitialize()
    {
        _directory = Path.Combine(Path.GetTempPath(), "FixClientTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        foreach (string file in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "TestData")))
        {
            File.Copy(file, Path.Combine(_directory, Path.GetFileName(file)));
        }
    }

    [TestCleanup]
    public void TestCleanup()
    {
        Directory.Delete(_directory, recursive: true);
    }

    FixClient.Session ReadSession(string name)
    {
        var session = new FixClient.Session(new System.Windows.Forms.Control())
        {
            FileName = Path.Combine(_directory, name)
        };
        session.Read();
        return session;
    }

    [TestMethod]
    public void TestReadSessionFile()
    {
        using var session = ReadSession("INITIATOR-ACCEPTOR.session");

        Assert.AreEqual(Fix.Behaviour.Initiator, session.Behaviour);
        Assert.AreEqual("", session.BindHost);
        Assert.AreEqual(0, session.BindPort);
        Assert.AreEqual("192.168.0.103", session.Host);
        Assert.AreEqual(8089, session.Port);
        Assert.AreEqual(3, session.NextClOrdId);
        Assert.AreEqual(2, session.NextAllocId);
        Assert.IsFalse(session.AppendDateToClOrdID);
        Assert.AreEqual(Fix.Behaviour.Initiator, session.OrderBehaviour);
        Assert.AreSame(Fix.Dictionary.Versions.FIXT_1_1, session.BeginString);
        Assert.AreSame(Fix.Dictionary.Versions.FIX_5_0SP2, session.DefaultApplVerId);
        Assert.AreEqual(Fix.Behaviour.Initiator, session.LogonBehaviour);
        Assert.AreEqual(2, session.TestRequestDelay);
        Assert.AreEqual("INITIATOR", session.SenderCompId);
        Assert.AreEqual("ACCEPTOR", session.TargetCompId);
        Assert.AreEqual(30, session.HeartBtInt);
        Assert.AreEqual("yyyyMMdd-HH:mm:ss.fff", session.OutgoingTimestampFormat);
        Assert.AreEqual(5, session.IncomingSeqNum);
        Assert.AreEqual(6, session.OutgoingSeqNum);
        Assert.IsTrue(session.FragmentMessages);
        Assert.IsTrue(session.ValidateMessages);
    }

    [TestMethod]
    public void TestReadAcceptorSessionFile()
    {
        using var session = ReadSession("ACCEPTOR-INITIATOR.session");

        Assert.AreEqual(Fix.Behaviour.Acceptor, session.Behaviour);
        Assert.AreEqual(Fix.Behaviour.Acceptor, session.OrderBehaviour);
        Assert.AreEqual(Fix.Behaviour.Initiator, session.LogonBehaviour);
        Assert.AreEqual(521, session.IncomingSeqNum);
        Assert.AreEqual(521, session.OutgoingSeqNum);
        Assert.AreEqual("ACCEPTOR", session.SenderCompId);
    }

    [TestMethod]
    public void TestReadFiltersFile()
    {
        using var session = ReadSession("INITIATOR-ACCEPTOR.session");

        Assert.IsFalse(session.IsMessageVisible("0"));
        Assert.IsFalse(session.IsMessageVisible("1"));
        Assert.IsTrue(session.IsMessageVisible("D"));
        CollectionAssert.AreEquivalent(OrderFieldFilters, session.FieldFilters("D")!.ToArray());
    }

    [TestMethod]
    public void TestReadCustomFieldsFile()
    {
        using var session = ReadSession("INITIATOR-ACCEPTOR.session");

        Assert.HasCount(2, session.CustomFields);
        Assert.AreEqual("TraderId", session.CustomFields[5001].Name);
        Assert.AreEqual("Desk", session.CustomFields[5002].Name);
    }

    [TestMethod]
    public void TestWrittenFilesReadBackTheSame()
    {
        string copy = Path.Combine(_directory, "COPY.session");

        using (var original = ReadSession("INITIATOR-ACCEPTOR.session"))
        {
            original.FileName = copy;
            original.Write();
            original.WriteFilters();
            original.WriteCustomFields();
        }

        using var expected = ReadSession("INITIATOR-ACCEPTOR.session");
        using var actual = ReadSession("COPY.session");

        // Every property the original file stores.
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(_directory, "INITIATOR-ACCEPTOR.session")));
        foreach (JsonProperty stored in document.RootElement.EnumerateObject())
        {
            PropertyInfo property = typeof(FixClient.Session).GetProperty(stored.Name) ?? throw new AssertFailedException($"No property {stored.Name}");
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), stored.Name);
        }

        Assert.IsFalse(actual.IsMessageVisible("0"));
        CollectionAssert.AreEquivalent(expected.FieldFilters("D")!.ToArray(), actual.FieldFilters("D")!.ToArray());
        CollectionAssert.AreEquivalent(expected.CustomFields.Keys, actual.CustomFields.Keys);
    }

    [TestMethod]
    public void TestWrittenSessionFileFormat()
    {
        string copy = Path.Combine(_directory, "COPY.session");

        using (var original = ReadSession("INITIATOR-ACCEPTOR.session"))
        {
            original.FileName = copy;
            original.Write();
        }

        // Enumerations and FIX versions are written as names so the files stay readable by earlier
        // versions of FixClient.
        using var document = JsonDocument.Parse(File.ReadAllText(copy));
        JsonElement root = document.RootElement;
        Assert.AreEqual("Initiator", root.GetProperty("Behaviour").GetString());
        Assert.AreEqual("Initiator", root.GetProperty("LogonBehaviour").GetString());
        Assert.AreEqual("FIXT.1.1", root.GetProperty("BeginString").GetString());
        Assert.AreEqual("FIX.5.0SP2", root.GetProperty("DefaultApplVerId").GetString());
        Assert.AreEqual(8089, root.GetProperty("Port").GetInt32());
        Assert.AreEqual("yyyyMMdd-HH:mm:ss.fff", root.GetProperty("OutgoingTimestampFormat").GetString());
        Assert.IsFalse(root.TryGetProperty("FileName", out _));
    }
}
