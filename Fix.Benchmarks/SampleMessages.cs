namespace Fix.Benchmarks;

public enum SampleMessage
{
    NewOrderSingle,
    ExecutionReport,
    LargeData,
}

//
// Messages shaped like typical traffic for the benchmarks that process whole messages.
//
//   NewOrderSingle  17 fields
//   ExecutionReport 25 fields
//   LargeData       a News message carrying 4 KB of RawData
//
static class SampleMessages
{
    public static Message Create(SampleMessage kind) => kind switch
    {
        SampleMessage.NewOrderSingle => new Message(new FieldCollection
        {
            {8, "FIX.4.4"}, {9, "0"}, {35, "D"}, {49, "INITIATOR"}, {56, "ACCEPTOR"}, {34, "1234"},
            {52, "20261115-08:15:30.123"}, {11, "ORD-20261115-000123"}, {21, "1"}, {55, "BHP"},
            {54, "1"}, {60, "20261115-08:15:30.120"}, {38, "10000"}, {40, "2"}, {44, "45.32"},
            {59, "0"}, {1, "ACCOUNT-01"},
        }),
        SampleMessage.ExecutionReport => new Message(new FieldCollection
        {
            {8, "FIX.4.4"}, {9, "0"}, {35, "8"}, {49, "ACCEPTOR"}, {56, "INITIATOR"}, {34, "5678"},
            {52, "20261115-08:15:30.456"}, {37, "EXCH-0000098765"}, {11, "ORD-20261115-000123"},
            {17, "EXEC-0000123456"}, {150, "F"}, {39, "1"}, {55, "BHP"}, {54, "1"}, {38, "10000"},
            {44, "45.32"}, {32, "2500"}, {31, "45.31"}, {151, "7500"}, {14, "2500"}, {6, "45.31"},
            {60, "20261115-08:15:30.450"}, {40, "2"}, {59, "0"}, {1, "ACCOUNT-01"},
        }),
        SampleMessage.LargeData => CreateLargeData(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };

    static Message CreateLargeData()
    {
        byte[] raw = Enumerable.Range(0, 4096).Select(i => (byte)i).ToArray();
        return new Message(new FieldCollection
        {
            {8, "FIX.4.4"}, {9, "0"}, {35, "B"}, {49, "ACCEPTOR"}, {56, "INITIATOR"}, {34, "42"},
            {52, "20261115-08:15:30.456"}, {148, "Market update"},
            {95, raw.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)},
            new Field(96, Convert.ToBase64String(raw)) { Data = true },
        });
    }
}
