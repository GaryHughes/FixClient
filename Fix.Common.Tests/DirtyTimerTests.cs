using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fix.Common.Tests;

[TestClass]
public class DirtyTimerTests
{
    [TestMethod]
    public void TestFlushRaisesDirtyOnlyWhenDirty()
    {
        using var timer = new DirtyTimer();
        int raised = 0;
        timer.Dirty += sender => ++raised;

        timer.Flush();
        Assert.AreEqual(0, raised);

        timer.SetDirty();
        timer.Flush();
        Assert.AreEqual(1, raised);

        timer.Flush();
        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void TestFlushAfterDispose()
    {
        var timer = new DirtyTimer();
        int raised = 0;
        timer.Dirty += sender => ++raised;
        timer.Start(60000, 60000);
        timer.SetDirty();
        timer.Dispose();

        timer.Flush();
        Assert.AreEqual(1, raised);
    }
}
