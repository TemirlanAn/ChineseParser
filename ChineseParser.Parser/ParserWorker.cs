using ChineseParser.Parser.Parsers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChineseParser.Parser
{
    public class ParserWorker
    {
        public ParserWorker()
        {
        }

        public async Task<ParseResult> Parse(string charackter)
        {
            if (string.IsNullOrWhiteSpace(charackter))
                throw new ArgumentException(nameof(charackter) + "can't be null or empty");
            var result = await new TrainChineseParser(charackter).Parse();
            result.AudioLocalUri = await new BkrsParser(charackter).DownloadAudioFile();
            return result;
        }
    }
}
