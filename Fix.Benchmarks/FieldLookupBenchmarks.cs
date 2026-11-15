using BenchmarkDotNet.Attributes;

namespace Fix.Benchmarks;

//
// VersionFieldCollection.TryGetValue(string name) compared with the search it replaced, which
// scanned the Fields array in tag order calling ToLower on both names for each entry. The array
// is indexed by tag so it includes placeholder entries for unused tags: 957 entries for FIX.4.4
// and 50,003 for FIX.5.0SP2, of which 912 and 5,938 are real fields.
//
//   Account   tag 1, the first field
//   OrderQty  tag 38
//   Last      the field with the highest tag: LegInterestAccrualDate (956) in FIX.4.4 and
//             BatchProcessMode (50002) in FIX.5.0SP2
//   NotAField not defined, so a search examines every entry
//
[MemoryDiagnoser]
public class FieldLookupBenchmarks
{
    [Params("FIX.4.4", "FIX.5.0SP2")]
    public string BeginString { get; set; } = string.Empty;

    [Params("Account", "OrderQty", "Last", "NotAField")]
    public string Field { get; set; } = string.Empty;

    Dictionary.VersionFieldCollection _fields = null!;
    string _name = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _fields = Dictionary.Versions[BeginString]?.Fields ?? throw new InvalidOperationException($"Unknown version {BeginString}");
        _name = Field == "Last" ? _fields.Fields.Last(Dictionary.IsValid).Name : Field;
        // Build the index outside the measurements; FieldIndexBuildBenchmarks measures that.
        _fields.TryGetValue(_name, out _);
    }

    [Benchmark(Baseline = true)]
    public Dictionary.VersionField? LinqSearch()
    {
        // The implementation VersionFieldCollection used before the FrozenDictionary.
#pragma warning disable CA1304, CA1311, CA1862 // Deliberately the original culture sensitive comparison
        return (from candidate in _fields.Fields
                where candidate?.Name.ToLower() == _name.ToLower()
                select candidate).FirstOrDefault();
#pragma warning restore CA1304, CA1311, CA1862
    }

    [Benchmark]
    public Dictionary.VersionField? FrozenDictionary()
    {
        return _fields.TryGetValue(_name, out var field) ? field : null;
    }
}

//
// The name index is built on the first lookup by name. This measures that first lookup on a newly
// constructed field collection, which is the one-off cost of the change.
//
[MemoryDiagnoser]
[InvocationCount(1)]
public class FieldIndexBuildBenchmarks
{
    [Params("FIX.4.4", "FIX.5.0SP2")]
    public string BeginString { get; set; } = string.Empty;

    Type _collectionType = null!;
    Dictionary.VersionFieldCollection _fields = null!;

    [GlobalSetup]
    public void Setup()
    {
        _collectionType = Dictionary.Versions[BeginString]?.Fields.GetType() ?? throw new InvalidOperationException($"Unknown version {BeginString}");
    }

    [IterationSetup]
    public void CreateCollection()
    {
        _fields = (Dictionary.VersionFieldCollection)Activator.CreateInstance(_collectionType, nonPublic: true)!;
    }

    [Benchmark]
    public bool FirstLookup() => _fields.TryGetValue("OrderQty", out _);
}
