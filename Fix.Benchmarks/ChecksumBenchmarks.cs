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
// The messages are described in SampleMessages.
//
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class ChecksumBenchmarks
{
    [Params(SampleMessage.NewOrderSingle, SampleMessage.ExecutionReport, SampleMessage.LargeData)]
    public SampleMessage Message { get; set; }

    Fix.Message _message = null!;

    [GlobalSetup]
    public void Setup()
    {
        _message = SampleMessages.Create(Message);
        _message.Fields.Add(10, "000");
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
