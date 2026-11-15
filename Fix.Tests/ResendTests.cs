using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FixTests;

[TestClass]
public class ResendTests
{
    [TestMethod]
    [DataRow("FIX.4.0", 999999, 0)]
    [DataRow("FIX.4.1", 999999, 0)]
    [DataRow("FIX.4.2", 999999, 999999)]
    [DataRow("FIXT.1.1", 999999, 999999)]
    [DataRow("FIX.4.0", 9999, 9999)]
    [DataRow("FIX.4.0", 0, 0)]
    [DataRow("FIX.4.1", 5, 5)]
    public void TestLegacyInfiniteEndSeqNoIsNormalised(string beginString, int endSeqNo, int expected)
    {
        Assert.AreEqual(expected, Fix.Session.NormaliseEndSeqNo(beginString, endSeqNo));
    }
}
