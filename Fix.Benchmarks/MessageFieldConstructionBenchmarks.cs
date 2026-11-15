using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;

namespace Fix.Benchmarks;

//
// Builds every message field definition of a version the way the generated dictionary did before
// and after sharing field definitions, which is most of the work and memory of loading the
// dictionary.
//
//   Copies: the previous generated code, new MessageField(new FIX_5_0SP2.OrderQty(), ...), which
//           constructed a new VersionField, with its own Pedigree and dictionary of values, for
//           every field of every message.
//   Shared: the current generated code, new MessageField(FIX_5_0SP2.Fields.OrderQty, ...).
//
// The field types are constructed through compiled delegates so the comparison is not distorted
// by reflection. Allocated shows the memory the definitions hold, as they live as long as the
// process.
//
[MemoryDiagnoser]
public class MessageFieldConstructionBenchmarks
{
    [Params("FIX.4.2", "FIX.4.4", "FIX.5.0SP2")]
    public string BeginString { get; set; } = string.Empty;

    (Func<Dictionary.VersionField> Create, Dictionary.VersionField Shared, bool Required, int Depth)[] _definitions = [];

    [GlobalSetup]
    public void Setup()
    {
        var version = Dictionary.Versions[BeginString] ?? throw new InvalidOperationException($"Unknown version {BeginString}");
        var factories = new System.Collections.Generic.Dictionary<Type, Func<Dictionary.VersionField>>();

        _definitions = version.Messages
            .SelectMany(message => message.Fields)
            .Select(field => (CreateFactory(factories, field.Field.GetType()), field.Field, field.Required, field.Depth))
            .ToArray();
    }

    static Func<Dictionary.VersionField> CreateFactory(System.Collections.Generic.Dictionary<Type, Func<Dictionary.VersionField>> factories, Type type)
    {
        if (!factories.TryGetValue(type, out var factory))
        {
            factory = Expression.Lambda<Func<Dictionary.VersionField>>(Expression.New(type)).Compile();
            factories.Add(type, factory);
        }
        return factory;
    }

    [Benchmark(Baseline = true)]
    public Dictionary.MessageField[] Copies()
    {
        var fields = new Dictionary.MessageField[_definitions.Length];
        for (int i = 0; i < fields.Length; ++i)
        {
            var definition = _definitions[i];
            fields[i] = new Dictionary.MessageField(definition.Create(), definition.Required, definition.Depth);
        }
        return fields;
    }

    [Benchmark]
    public Dictionary.MessageField[] Shared()
    {
        var fields = new Dictionary.MessageField[_definitions.Length];
        for (int i = 0; i < fields.Length; ++i)
        {
            var definition = _definitions[i];
            fields[i] = new Dictionary.MessageField(definition.Shared, definition.Required, definition.Depth);
        }
        return fields;
    }
}
