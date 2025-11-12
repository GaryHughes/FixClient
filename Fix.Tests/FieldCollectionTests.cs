using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Fix.Dictionary;

namespace FixTests;

[TestClass]
public class FieldCollectionTests
{
    [TestInitialize]
    public void Initialise()
    {
        string[,] fields =
        {
            { "8", "FIX.4.0" },
            { "9", "127" },
            { "35", "C" },
            { "49", "ITGHK" },
            { "56", "KODIAK_KGEHVWAP" },
            { "34", "4" },
            { "52", "20090630-23:37:12" },
            { "94", "0" },
            { "33", "1" },
            { "58", "RemotedHost#Name=gateQA-p01,Ip=10.132.3.125,Port=7081#" },
            { "10", "128" }
        };
        Collection = new Fix.FieldCollection(fields);
        Assert.AreEqual(11, Collection.Count);
    }

    Fix.FieldCollection Collection { get; set; } = new Fix.FieldCollection();

    [TestMethod]
    public void TestFindFromByTagWithNegativeIndex()
    {
        Assert.Throws<IndexOutOfRangeException>(() => Collection.FindFrom(FIX_5_0SP2.Fields.BeginString.Tag, -1));
    }

    [TestMethod]
    public void TestFindFromByTagWithIndexOffTheEnd()
    {
        Assert.Throws<IndexOutOfRangeException>(() => Collection.FindFrom(FIX_5_0SP2.Fields.BeginString.Tag, 11));
    }

    [TestMethod]
    public void TestFindFromRefByTag()
    {
        int index = 0;
        Fix.Field? field = Collection.FindFrom(FIX_5_0SP2.Fields.BeginString.Tag, ref index);
        Assert.AreEqual(0, index);
        Assert.IsNotNull(field);
        Assert.AreEqual(8, field?.Tag);
        Assert.AreEqual("FIX.4.0", field?.Value);
        index = 1;
        field = Collection.FindFrom(FIX_5_0SP2.Fields.BeginString.Tag, ref index);
        Assert.AreEqual(-1, index);
        Assert.IsNull(field);
        index = 0;
        field = Collection.FindFrom(FIX_5_0SP2.Fields.TargetCompID.Tag, ref index);
        Assert.AreEqual(4, index);
        Assert.IsNotNull(field);
    }

    [TestMethod]
    public void TestFindFromByTag()
    {
        Fix.Field? field = Collection.FindFrom(FIX_5_0SP2.Fields.BeginString.Tag, 0);
        Assert.IsNotNull(field);
        Assert.AreEqual(8, field?.Tag);
        Assert.AreEqual("FIX.4.0", field?.Value);
        field = Collection.FindFrom(FIX_5_0SP2.Fields.BeginString.Tag, 1);
        Assert.IsNull(field);
    }

    [TestMethod]
    public void TestFindFromFirstRefByDefinition()
    {
        int index = 0;
        Fix.Field? field = Collection.FindFrom(FIX_5_0SP2.Fields.BeginString, ref index);
        Assert.AreEqual(0, index);
        Assert.IsNotNull(field);
        Assert.AreEqual(8, field?.Tag);
        Assert.AreEqual("FIX.4.0", field?.Value);
        index = 1;
        field = Collection.FindFrom(FIX_5_0SP2.Fields.BeginString, ref index);
        Assert.AreEqual(-1, index);
        Assert.IsNull(field);
    }

    [TestMethod]
    public void TestFindFromFirstByDefinition()
    {
        Fix.Field? field = Collection.FindFrom(FIX_5_0SP2.Fields.BeginString, 0);
        Assert.IsNotNull(field);
        Assert.AreEqual(8, field?.Tag);
        Assert.AreEqual("FIX.4.0", field?.Value);
        field = Collection.FindFrom(FIX_5_0SP2.Fields.BeginString, 1);
        Assert.IsNull(field);
    }

    [TestMethod]
    public void TestFindFirstByTag()
    {
        Fix.Field? field = Collection.Find(FIX_5_0SP2.Fields.BeginString.Tag);
        Assert.IsNotNull(field);
        Assert.AreEqual(8, field?.Tag);
        Assert.AreEqual("FIX.4.0", field?.Value);
    }

    [TestMethod]
    public void TestFindFirstByDefinition()
    {
        Fix.Field? field = Collection.Find(FIX_5_0SP2.Fields.BeginString);
        Assert.IsNotNull(field);
        Assert.AreEqual(8, field?.Tag);
        Assert.AreEqual("FIX.4.0", field?.Value);
    }

    [TestMethod]
    public void TestFindLastByTag()
    {
        Fix.Field? field = Collection.Find(FIX_5_0SP2.Fields.CheckSum.Tag);
        Assert.IsNotNull(field);
        Assert.AreEqual(10, field?.Tag);
        Assert.AreEqual("128", field?.Value);
    }

    [TestMethod]
    public void TestFindLastByDefinition()
    {
        Fix.Field? field = Collection.Find(FIX_5_0SP2.Fields.CheckSum);
        Assert.IsNotNull(field);
        Assert.AreEqual(10, field?.Tag);
        Assert.AreEqual("128", field?.Value);
    }

    [TestMethod]
    public void TestRepeat()
    {
        Collection.Repeat(7, 2);
        Assert.AreEqual(13, Collection.Count);
        Assert.AreEqual("0", Collection[9].Value);
        Assert.AreEqual("1", Collection[10].Value);
    }

    [TestMethod]
    public void TestRemove()
    {
        Assert.AreEqual("ITGHK", Collection[3].Value);
        Collection.Remove(FIX_5_0SP2.Fields.SenderCompID);
        Assert.AreEqual(10, Collection.Count);
        Assert.AreEqual("KODIAK_KGEHVWAP", Collection[3].Value);
    }
}

