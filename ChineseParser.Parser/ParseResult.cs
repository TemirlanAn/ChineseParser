using System.Collections.Generic;

namespace ChineseParser.Parser
{
    public class ParseResult
    {
        public string Charackter { get; set; }
        public string Pinyn { get; set; }
        public string Translate { get; set; }
        public string AudioLocalUri { get; set; }
        public IEnumerable<string> WriteOrderGifLocalUries { get; set; }
    }
}
