using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FixTests;

[TestClass]
public class NonClosingStreamDecoratorTests
{
    [TestMethod]
    public void TestDisposeLeavesTheInnerStreamOpen()
    {
        using var inner = new MemoryStream();
        var decorator = new Fix.NonClosingStreamDecorator(inner);
        decorator.WriteByte(1);
        decorator.Dispose();
        Assert.IsTrue(inner.CanRead);
        Assert.AreEqual(1, inner.Length);
    }

    [TestMethod]
    public void TestCloseLeavesTheInnerStreamOpen()
    {
        using var inner = new MemoryStream();
        var decorator = new Fix.NonClosingStreamDecorator(inner);
        decorator.Close();
        Assert.IsTrue(inner.CanWrite);
    }
}
