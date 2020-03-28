using ChineseParser.Parser.Parsers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChineseParser.Parser
{
    public class ParserWorker
    {
        private readonly string _charackter;

        public ParserWorker(string charackter)
        {
            if (string.IsNullOrWhiteSpace(charackter))
                throw new ArgumentException(nameof(charackter) + "can't be null or empty");
            _charackter = charackter;
        }

        public async Task<ParseResult> Parse()
        {
            var result = await new TrainChineseParser(_charackter).Parse();
            result.AudioLocalUri = await new BkrsParser(_charackter).DownloadAudioFile();
            return result;
        }
    }
}
