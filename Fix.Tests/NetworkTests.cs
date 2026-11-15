using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FixTests;

[TestClass]
public class NetworkTests
{
    [TestMethod]
    public void TestGetAddressWithNoHost()
    {
        var ex = Assert.Throws<ArgumentException>(() => Fix.Network.GetAddress(""));
        Assert.AreEqual("name", ex.ParamName);
    }

    [TestMethod]
    public void TestGetLocalAddressWithNoHost()
    {
        Assert.IsNull(Fix.Network.GetLocalAddress("  "));
    }

    [TestMethod]
    public void TestGetLocalAddressForLoopback()
    {
        Assert.AreEqual(System.Net.IPAddress.Loopback, Fix.Network.GetLocalAddress("127.0.0.1"));
    }

    [TestMethod]
    public void TestGetLocalAddressForAnAddressThatIsNotLocal()
    {
        // An address from the TEST-NET-1 documentation range, which is never assigned to a host.
        var ex = Assert.Throws<ArgumentException>(() => Fix.Network.GetLocalAddress("192.0.2.1"));
        Assert.AreEqual("name", ex.ParamName);
    }
}
