using System.ComponentModel;
using System.Linq;

namespace Fix;

//
// Makes PropertyDescriptor.IsReadOnly for a Session ask the session instance via
// Session.IsPropertyReadOnly. This lets the property grid lock and unlock properties
// based on the current values of other properties without mutating ReadOnlyAttribute
// instances, which TypeDescriptor caches and shares between every instance of a type.
//
sealed class SessionTypeDescriptionProvider : TypeDescriptionProvider
{
    public SessionTypeDescriptionProvider()
    : base(TypeDescriptor.GetProvider(typeof(object)))
    {
    }

    public override ICustomTypeDescriptor? GetTypeDescriptor(Type objectType, object? instance)
    {
        var parent = base.GetTypeDescriptor(objectType, instance);
        return instance is Session session ? new SessionTypeDescriptor(parent, session) : parent;
    }

    sealed class SessionTypeDescriptor : CustomTypeDescriptor
    {
        readonly Session _session;

        public SessionTypeDescriptor(ICustomTypeDescriptor? parent, Session session)
        : base(parent)
        {
            _session = session;
        }

        public override PropertyDescriptorCollection GetProperties() => Wrap(base.GetProperties());

        public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes) => Wrap(base.GetProperties(attributes));

        PropertyDescriptorCollection Wrap(PropertyDescriptorCollection properties)
        {
            var wrapped = properties.Cast<PropertyDescriptor>()
                                    .Select(property => (PropertyDescriptor)new SessionPropertyDescriptor(property, _session))
                                    .ToArray();
            return new PropertyDescriptorCollection(wrapped, true);
        }
    }

    sealed class SessionPropertyDescriptor : PropertyDescriptor
    {
        readonly PropertyDescriptor _inner;
        readonly Session _session;

        public SessionPropertyDescriptor(PropertyDescriptor inner, Session session)
        : base(inner)
        {
            _inner = inner;
            _session = session;
        }

        public override bool IsReadOnly => _inner.IsReadOnly || _session.IsPropertyReadOnly(Name);

        public override Type ComponentType => _inner.ComponentType;
        public override Type PropertyType => _inner.PropertyType;
        public override TypeConverter Converter => _inner.Converter;
        public override bool CanResetValue(object component) => _inner.CanResetValue(component);
        public override object? GetValue(object? component) => _inner.GetValue(component);
        public override void ResetValue(object component) => _inner.ResetValue(component);
        public override void SetValue(object? component, object? value) => _inner.SetValue(component, value);
        public override bool ShouldSerializeValue(object component) => _inner.ShouldSerializeValue(component);
        public override bool SupportsChangeEvents => _inner.SupportsChangeEvents;
        public override void AddValueChanged(object component, EventHandler handler) => _inner.AddValueChanged(component, handler);
        public override void RemoveValueChanged(object component, EventHandler handler) => _inner.RemoveValueChanged(component, handler);
    }
}
