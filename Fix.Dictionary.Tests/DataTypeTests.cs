using Fix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FixDictionary.Tests;

[TestClass]
public class DataTypeTests
{
    [TestMethod]
    public void TestIntDefinition()
    {
        Assert.AreEqual("Int", Dictionary.FIX_4_4.DataTypes.Int.Name);
        // TODO
        // Assert.IsNull(Dictionary.FIX_4_4.DataTypes.Int.BaseType);
    }

    [TestMethod]
    public void TestLengthDefinition()
    {
        Assert.AreEqual("Length", Dictionary.FIX_4_4.DataTypes.Length.Name);
        // TODO
        // Assert.IsNotNull(Dictionary.FIX_4_4.DataTypes.Length.BaseType);
        Assert.AreEqual(Dictionary.FIX_4_4.DataTypes.Int, Dictionary.FIX_4_4.DataTypes.Int);
    }

    [TestMethod]
    public void TestDataTypeCollections()
    {
        Assert.AreEqual(20, Dictionary.FIX_4_2.DataTypes.Count);
        Assert.AreEqual(38, Dictionary.FIX_5_0SP2.DataTypes.Count);
    }
}

