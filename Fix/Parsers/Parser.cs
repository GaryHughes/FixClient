namespace Fix;

public static class Parser
{
    // The parsers keep state while they read a stream (FormattedLogParser remembers the FIX version
    // it has seen for example) so every parse gets its own instances.
    static LogParser[] CreateParsers() =>
    [
        new Parsers.GenericLogParser(),
        new Parsers.FormattedLogParser(),
        new Parsers.KlsLogParser()
    ];

    public static async IAsyncEnumerable<Message> Parse(Stream stream)
    {
        long position = stream.Position;

        using var decorator = new NonClosingStreamDecorator(stream);

        foreach (var parser in CreateParsers())
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
            throw new ArgumentException($"Expected a URI with a 'file' scheme and got '{uri.Scheme}'", nameof(uri));
        }

        using FileStream stream = new(uri.LocalPath, FileMode.Open);

        await foreach (var message in Parse(stream).ConfigureAwait(false))
        {
            yield return message;
        }
    }
}

