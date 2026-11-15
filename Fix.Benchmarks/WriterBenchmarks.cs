using System.Text;
using BenchmarkDotNet.Attributes;
using static Fix.Dictionary;

namespace Fix.Benchmarks;

//
// Writer.Write for each sample message, compared with the previous Writer, which formatted every
// field into a new string and encoded it into a new byte array, and decoded data fields into
// another. Both compute the body length and checksum the same way, which is part of the cost of
// every write. The messages are described in SampleMessages and are written to Stream.Null.
//
[MemoryDiagnoser]
public class WriterBenchmarks : IDisposable
{
    [Params(SampleMessage.NewOrderSingle, SampleMessage.ExecutionReport, SampleMessage.LargeData)]
    public SampleMessage Message { get; set; }

    Fix.Message _message = null!;
    Writer _writer = null!;
    PreviousWriter _previous = null!;

    [GlobalSetup]
    public void Setup()
    {
        _message = SampleMessages.Create(Message);
        _writer = new Writer(Stream.Null, leaveOpen: true);
        _previous = new PreviousWriter(Stream.Null);
    }

    [GlobalCleanup]
    public void Dispose()
    {
        _writer.Dispose();
        _previous.Dispose();
        GC.SuppressFinalize(this);
    }

    [Benchmark(Baseline = true)]
    public void WritePrevious() => _previous.Write(_message);

    [Benchmark]
    public void Write() => _writer.Write(_message);

    // Writer.Write as it was before, without the events, which have no subscribers here.
    sealed class PreviousWriter(Stream stream) : IDisposable
    {
        readonly BinaryWriter _writer = new(stream, Encoding.Latin1, leaveOpen: true);

        public void Write(Fix.Message message)
        {
            message.Fields.Set(FIX_5_0SP2.Fields.BodyLength.Tag, message.ComputeBodyLength());
            message.Fields.Remove(FIX_5_0SP2.Fields.CheckSum);
            message.Fields.Add(FIX_5_0SP2.Fields.CheckSum.Tag, message.ComputeCheckSum());

            using MemoryStream buffer = new();
            using (BinaryWriter writer = new(buffer, Encoding.Latin1, true))
            {
                WriteFields(writer, message);
            }

            lock (_writer)
            {
                _writer.Write(buffer.GetBuffer(), 0, (int)buffer.Length);
            }
            _writer.Flush();
        }

#pragma warning disable CA1305 // Deliberately the original culture sensitive formatting
        static void WriteFields(BinaryWriter writer, Fix.Message message)
        {
            foreach (Field field in message.Fields)
            {
                if (field.Tag == FIX_5_0SP2.Fields.CheckSum.Tag)
                    continue;

                if (field.Data)
                {
                    byte[] bytes = Convert.FromBase64String(field.Value);
                    writer.Write(Encoding.Latin1.GetBytes(string.Format("{0}=", field.Tag)));
                    writer.Write(bytes);
                    writer.Write(Encoding.Latin1.GetBytes("\x01"));
                }
                else
                {
                    writer.Write(Encoding.Latin1.GetBytes($"{field.Tag}={field.Value}\x01"));
                }
            }

            writer.Write(Encoding.Latin1.GetBytes($"{(int)FIX_5_0SP2.Fields.CheckSum.Tag}={message.CheckSum}\x01"));
        }
#pragma warning restore CA1305

        public void Dispose() => _writer.Dispose();
    }
}
