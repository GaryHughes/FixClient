using Microsoft.VisualStudio.TestTools.UnitTesting;

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
}
