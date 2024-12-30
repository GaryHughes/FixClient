/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: KlsLogParser.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////

using System.IO;
using System.Threading.Tasks;

namespace Fix.Parsers;

public class KlsLogParser : LogParser
{
    //
    //	A KLS log looks like this
    //
    //	12/24/09 00:26:38 log _TP1_STRATGEHASF ITG 12/24/09 11:26:38 
    //	<Sender:KODIAK,Target:server> CLoggingMessageHandler::notifyOutgoing() message=
    //	8=FIX.4.09=7135=149=KODIAK56=server34=8852=20091224-00:26:38112=2009122411263810=046
    //
    //	12/24/09 00:27:10 log _TP1_STRATGEHASF ITG 12/24/09 11:27:10
    //	<Sender:KODIAK,Target:server> CLoggingMessageHandler::notifyOutgoing() message=
    //	8=FIX.4.09=7135=149=KODIAK56=server34=9052=20091224-00:27:10112=2009122411271010=021
    //
    //	12/24/09 00:04:31	log	_TP1_STRATGEHASF	ITG	12/24/09 11:04:30
    //	<Sender:KODIAK,Target:server> CLoggingMessageHandler::processIncoming() message=
    //	8=FIX.4.09=6335=A49=server56=KODIAK34=152=20091224-00:04:3098=0108=3010=114
    //
    protected override async Task<Message?> ParseMessage(TextReader reader)
    {
        Message message;

        for (; ; )
        {
            var line = await reader.ReadLineAsync();

            if (line == null)
                return null;

            bool incoming;

            if (line.Contains("CLoggingMessageHandler::notifyOutgoing() message="))
            {
                incoming = false;
            }
            else if (line.Contains("CLoggingMessageHandler::processIncoming() message="))
            {
                incoming = true;
            }
            else
            {
                continue;
            }

            line = reader.ReadLine();

            if (string.IsNullOrEmpty(line))
                continue;

            try
            {
                message = new Message(line)
                {
                    Incoming = incoming
                };
            }
            catch (Exception)
            {
                continue;
            }

            break;
        }

        return message;
    }
}

