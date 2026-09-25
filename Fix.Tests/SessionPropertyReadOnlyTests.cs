using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using static Fix.Dictionary;

namespace FixTests;

[TestClass]
public class SessionPropertyReadOnlyTests
{
    class TestSession : Fix.Session
    {
        public override bool IsPropertyReadOnly(string propertyName) =>
            propertyName == nameof(DefaultApplVerId) ? BeginString != Versions.FIXT_1_1 : base.IsPropertyReadOnly(propertyName);
    }

    static bool IsReadOnly(object component, string name)
    {
        var property = TypeDescriptor.GetProperties(component)[name];
        Assert.IsNotNull(property, $"Property {name} not found");
        return property.IsReadOnly;
    }

    [TestMethod]
    public void TestReadOnlyFollowsCurrentPropertyValues()
    {
        var session = new TestSession { BeginString = Versions.FIXT_1_1 };
        Assert.IsFalse(IsReadOnly(session, nameof(Fix.Session.DefaultApplVerId)));

        session.BeginString = Versions.FIX_4_4;
        Assert.IsTrue(IsReadOnly(session, nameof(Fix.Session.DefaultApplVerId)));

        session.BeginString = Versions.FIXT_1_1;
        Assert.IsFalse(IsReadOnly(session, nameof(Fix.Session.DefaultApplVerId)));
    }

    [TestMethod]
    public void TestReadOnlyIsPerInstance()
    {
        var fixt = new TestSession { BeginString = Versions.FIXT_1_1 };
        var fix44 = new TestSession { BeginString = Versions.FIX_4_4 };
        Assert.IsFalse(IsReadOnly(fixt, nameof(Fix.Session.DefaultApplVerId)));
        Assert.IsTrue(IsReadOnly(fix44, nameof(Fix.Session.DefaultApplVerId)));
    }

    [TestMethod]
    public void TestOtherPropertiesAreUnaffected()
    {
        var session = new TestSession { BeginString = Versions.FIX_4_4 };
        Assert.IsFalse(IsReadOnly(session, nameof(Fix.Session.BeginString)));
        Assert.IsFalse(IsReadOnly(session, nameof(Fix.Session.SenderCompId)));
    }

    [TestMethod]
    public void TestWrappedDescriptorsStillGetAndSetValues()
    {
        var session = new TestSession { BeginString = Versions.FIXT_1_1, SenderCompId = "A" };
        var property = TypeDescriptor.GetProperties(session)[nameof(Fix.Session.SenderCompId)];
        Assert.IsNotNull(property);
        Assert.AreEqual("A", property.GetValue(session));
        property.SetValue(session, "B");
        Assert.AreEqual("B", session.SenderCompId);
        Assert.IsNotNull(property.Converter);
    }

    [TestMethod]
    public void TestBaseSessionHasNoReadOnlyProperties()
    {
        var session = new Fix.Session { BeginString = Versions.FIX_4_4 };
        Assert.IsFalse(IsReadOnly(session, nameof(Fix.Session.DefaultApplVerId)));
    }
}
