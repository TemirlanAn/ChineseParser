using AngleSharp.Dom;
using AngleSharp;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.IO;

namespace ChineseParser.Parser.Parsers
{
    public class TrainChineseParser
    {
        private const string NotAviableCharackterPattern = "Trainchinese | Разбить предложение";
        private const string TrainChineseBaseUri = "https://www.trainchinese.com/v2/search.php?searchWord=search_word&rAp=276580852210&height=0&width=0&tcLanguage=ru";
        private readonly string _charackter;
        private readonly HttpClient _httpClient;

        public TrainChineseParser(string charackter)
        {
            if (string.IsNullOrWhiteSpace(charackter))
                throw new ArgumentException(nameof(charackter) + "can't be null or empty");
            _charackter = charackter;
            _httpClient = new HttpClient();
        }

        public async Task<ParseResult> Parse()
        {
            var parseResult = new ParseResult();
            var document = await OpenDocument();
            if (document.Title == NotAviableCharackterPattern)
                return new ParseResult();

            var element = document.QuerySelector("table.table.table-hover").QuerySelector("tbody");
            var word = element.Children.FirstOrDefault(Check);
            var wordId = word.QuerySelector("td").Attributes["onclick"].Value.Split('(')[1].Split(')')[0];

            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            document = await context.OpenAsync($"https://www.trainchinese.com/v2/wordDetails.php?rAp=0&wordId={wordId}&tcLanguage=ru");

            var pinyin = document.QuerySelector("div.pinyin")?.TextContent;
            var translate = document.QuerySelector("div.translation")?.TextContent;
            var gifUrls = GetStrokeOrderGifUrls(document);
            var localGifUrl = await SaveGifs(gifUrls);

            return new ParseResult
            {
                Charackter = _charackter,
                WriteOrderGifLocalUries = localGifUrl,
                Pinyn = pinyin,
                Translate = translate
            };
        }

        private async Task<IEnumerable<string>> SaveGifs(IEnumerable<StrokeOrderGif> urls)
        {
            var list = new List<string>();
            var localFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "strokeOrder");           
            if (!Directory.Exists(localFolderPath))
            {
                Directory.CreateDirectory(localFolderPath);
            }

            foreach (var url in urls)
            {
                var localFilePath = Path.Combine(localFolderPath, $"{url.Charackter}.gif");
                if (!File.Exists(localFilePath))
                {
                    var byteArray = await _httpClient.GetByteArrayAsync(url.Url);
                    await File.WriteAllBytesAsync(localFilePath, byteArray);
                }

                list.Add(localFilePath);
            }
            return list;
        }

        private IEnumerable<StrokeOrderGif> GetStrokeOrderGifUrls(IDocument document)
        {
            var urls = document.GetElementById("collapseWriting").QuerySelectorAll("img").Where(img => !img.Attributes["src"].Value.Contains("Traditional")).Select(s => new StrokeOrderGif
            {
                Url = s.Attributes["src"].Value.Replace("..", "https://www.trainchinese.com"),
                Charackter = _charackter
            });
            return urls.ToList();
        }

        private bool Check(IElement element)
        {
            var ch = string.Join("", element.QuerySelector("td").QuerySelector("div").Children.Select(s => s.InnerHtml.Trim()));
            string output1 = new string(_charackter.Where(c => !char.IsControl(c)).ToArray());
            string output2 = new string(ch.Where(c => !char.IsControl(c)).ToArray());

            var res = string.Compare(output1, output2, true, CultureInfo.GetCultureInfo("zh-CN"));
            return res > 0;
        }

        private async Task<IDocument> OpenDocument()
        {
            var uriPoint = TrainChineseBaseUri.Replace("search_word", _charackter);
            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            var document = await context.OpenAsync(uriPoint);
            return document;
        }
    }
}
