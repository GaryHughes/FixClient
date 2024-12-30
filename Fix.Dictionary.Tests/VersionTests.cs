using Fix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FixDictionary.Tests;

[TestClass]
public class VersionTests
{
    [TestMethod]
    public void TestCount()
    {
        Assert.AreEqual(5, Dictionary.Versions.Count);
    }

    [TestMethod]
    public void TestIndexer()
    {
        Assert.AreEqual("FIX.4.0", Dictionary.Versions.FIX_4_0.BeginString);
        Assert.AreEqual("FIX.4.2", Dictionary.Versions.FIX_4_2.BeginString);
        Assert.AreEqual("FIX.4.4", Dictionary.Versions.FIX_4_4.BeginString);
        Assert.AreEqual("FIXT.1.1", Dictionary.Versions.FIXT_1_1.BeginString);
        Assert.AreEqual("FIX.5.0SP2", Dictionary.Versions.FIX_5_0SP2.BeginString);
    }
}

