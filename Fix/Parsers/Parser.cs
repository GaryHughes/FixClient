using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fix;

public static class Parser
{
    static IEnumerable<LogParser> parsers = new LogParser[]
    {
        new Parsers.GenericLogParser(),
        new Parsers.FormattedLogParser(),
        new Parsers.KlsLogParser()
    };

    public static async IAsyncEnumerable<Message> Parse(Stream stream)
    {
        long position = stream.Position;

        using var decorator = new NonClosingStreamDecorator(stream);

        foreach (var parser in parsers)
        {
            bool foundMessages = false;

            await foreach (var message in parser.Parse(decorator).ConfigureAwait(false))
            {
                if (message is null)
                {
                    yield break;
                }

                foundMessages = true;

                yield return message;
            }

            if (foundMessages)
            {
                yield break;
            }

            stream.Seek(position, SeekOrigin.Begin);
        }
    }

    public static async IAsyncEnumerable<Message> Parse(Uri uri)
    {
        if (uri.Scheme != "file")
        {
            throw new ArgumentException("Exepected a URI with a 'file' scheme and got '{0}'", uri.Scheme);
        }

        using FileStream stream = new(uri.LocalPath, FileMode.Open);

        await foreach (var message in Parse(stream).ConfigureAwait(false))
        {
            yield return message;
        }
    }
}

