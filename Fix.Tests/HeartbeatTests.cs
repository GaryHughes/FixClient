using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FixTests;

[TestClass]
public class HeartbeatTests
{
    [TestMethod]
    [DataRow(30, 29000.0)]
    [DataRow(2, 1000.0)]
    [DataRow(1, 1000.0)]
    public void TestHeartbeatIsSentOneSecondBeforeTheIntervalExpires(int heartBtInt, double expectedMilliseconds)
    {
        Assert.AreEqual(expectedMilliseconds, Fix.Session.HeartbeatInterval(heartBtInt));
    }
}
