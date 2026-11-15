using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Fix.Benchmarks;

//
// Message.ComputeCheckSum and ComputeBodyLength run for every message sent, and the checksum for
// every message received. They call Field.ComputeCheckSum and Field.ComputeBodyLength for each
// field. Compares the current implementations with the previous ones, which formatted and encoded
// every field into new strings and byte arrays and summed them through LINQ.
//
//   NewOrderSingle  17 fields
//   ExecutionReport 25 fields
//   LargeData       a News message carrying 4 KB of RawData
//
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class ChecksumBenchmarks
{
    public enum Kind
    {
        NewOrderSingle,
        ExecutionReport,
        LargeData,
    }

    [Params(Kind.NewOrderSingle, Kind.ExecutionReport, Kind.LargeData)]
    public Kind Message { get; set; }

    Fix.Message _message = null!;

    [GlobalSetup]
    public void Setup()
    {
        _message = Message switch
        {
            Kind.NewOrderSingle => new Fix.Message(new FieldCollection
            {
                {8, "FIX.4.4"}, {9, "0"}, {35, "D"}, {49, "INITIATOR"}, {56, "ACCEPTOR"}, {34, "1234"},
                {52, "20261115-08:15:30.123"}, {11, "ORD-20261115-000123"}, {21, "1"}, {55, "BHP"},
                {54, "1"}, {60, "20261115-08:15:30.120"}, {38, "10000"}, {40, "2"}, {44, "45.32"},
                {59, "0"}, {1, "ACCOUNT-01"},
            }),
            Kind.ExecutionReport => new Fix.Message(new FieldCollection
            {
                {8, "FIX.4.4"}, {9, "0"}, {35, "8"}, {49, "ACCEPTOR"}, {56, "INITIATOR"}, {34, "5678"},
                {52, "20261115-08:15:30.456"}, {37, "EXCH-0000098765"}, {11, "ORD-20261115-000123"},
                {17, "EXEC-0000123456"}, {150, "F"}, {39, "1"}, {55, "BHP"}, {54, "1"}, {38, "10000"},
                {44, "45.32"}, {32, "2500"}, {31, "45.31"}, {151, "7500"}, {14, "2500"}, {6, "45.31"},
                {60, "20261115-08:15:30.450"}, {40, "2"}, {59, "0"}, {1, "ACCOUNT-01"},
            }),
            _ => CreateLargeData(),
        };
        _message.Fields.Add(10, "000");
    }

    static Fix.Message CreateLargeData()
    {
        byte[] raw = Enumerable.Range(0, 4096).Select(i => (byte)i).ToArray();
        return new Fix.Message(new FieldCollection
        {
            {8, "FIX.4.4"}, {9, "0"}, {35, "B"}, {49, "ACCEPTOR"}, {56, "INITIATOR"}, {34, "42"},
            {52, "20261115-08:15:30.456"}, {148, "Market update"},
            {95, raw.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)},
            new Field(96, Convert.ToBase64String(raw)) { Data = true },
        });
    }

    [Benchmark(Baseline = true), BenchmarkCategory("CheckSum")]
    public string CheckSumPrevious() => Previous.ComputeCheckSum(_message);

    [Benchmark, BenchmarkCategory("CheckSum")]
    public string CheckSum() => Fix.Message.ComputeCheckSum(_message);

    [Benchmark(Baseline = true), BenchmarkCategory("BodyLength")]
    public string BodyLengthPrevious() => Previous.ComputeBodyLength(_message);

    [Benchmark, BenchmarkCategory("BodyLength")]
    public string BodyLength() => Fix.Message.ComputeBodyLength(_message);

    // The implementations Field and Message used before.
    static class Previous
    {
        public static string ComputeCheckSum(Fix.Message message)
        {
            int value = message.Fields.Where(field => field.Tag != 10).Sum(FieldCheckSum) % 256;
#pragma warning disable CA1305 // Deliberately the original culture sensitive formatting
            return value.ToString("D3");
        }

        public static string ComputeBodyLength(Fix.Message message)
        {
            int bodyLength = 0;
            bool passedBodyLength = false;

            foreach (Field field in message.Fields)
            {
                if (field.Tag == 10)
                    continue;

                if (field.Tag == 9)
                {
                    passedBodyLength = true;
                    continue;
                }

                if (!passedBodyLength)
                    continue;

                bodyLength += FieldBodyLength(field);
            }

            return bodyLength.ToString();
        }

        static int FieldCheckSum(Field field)
        {
            int checksum = Encoding.Latin1.GetBytes(field.Tag + "=").Select(b => (int)b).Sum();

            if (!string.IsNullOrEmpty(field.Value))
            {
                if (field.Data)
                {
                    byte[] bytes = Convert.FromBase64String(field.Value);
                    checksum += bytes.Select(b => (int)b).Sum();
                }
                else
                {
                    checksum += Encoding.Latin1.GetBytes(field.Value).Select(b => (int)b).Sum();
                }
            }

            checksum += Encoding.Latin1.GetBytes("\x01").Select(b => (int)b).Sum();

            return checksum;
        }

        static int FieldBodyLength(Field field)
        {
            int bodyLength = Encoding.Latin1.GetBytes(string.Format("{0}=", field.Tag)).Length;
#pragma warning restore CA1305

            if (!string.IsNullOrEmpty(field.Value))
            {
                if (field.Data)
                {
                    byte[] bytes = Convert.FromBase64String(field.Value);
                    bodyLength += bytes.Length;
                }
                else
                {
                    bodyLength += field.Value.Length;
                }
            }

            bodyLength += 1;

            return bodyLength;
        }
    }
}
