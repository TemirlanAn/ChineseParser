using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChineseParser.Parser.Parsers
{
    public class BkrsParser
    {
        private const string BkrsBaseUri = "https://bkrs.info/slovo.php?ch=";
        private const string NotAviableCharackterPattern = "такого слова нет";
        private readonly string _charackter;
        private readonly HttpClient _httpClient;
        public BkrsParser(string charackter)
        {
            if (string.IsNullOrWhiteSpace(charackter))
                throw new ArgumentException(nameof(charackter) + "can't be null or empty");
            _charackter = charackter;
            _httpClient = new HttpClient();
        }

        public async Task<string> DownloadAudioFile()
        {
            var document = await OpenDocument();
            if (IsCharackterNotAviable(document))
            {
                return "";
            }

            var element = document?.GetElementById("ajax_search")?.QuerySelector("img.pointer");
            if (element == null)
                return "";

            var audioUrl = element?.Attributes["onclick"]?.Value;
            if (audioUrl == null)
                return "";

            try
            {
                var url = audioUrl.Split('\'')[1];
                var localFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "audiofiles");
                var localFilePath = Path.Combine(localFolderPath, $"{_charackter}.mp3");

                if (!Directory.Exists(localFolderPath))
                {
                    Directory.CreateDirectory(localFolderPath);
                }
                if (Directory.Exists(localFilePath))
                {
                    return localFilePath;
                }

                var byteArray = await _httpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(localFilePath, byteArray);
                return localFilePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return "";
            }
        }

        private bool IsCharackterNotAviable(IDocument document)
        {
            return document.GetElementById("ajax_search").QuerySelector("div.gray.pt10").InnerHtml == NotAviableCharackterPattern;            
        }
        private async Task<IDocument> OpenDocument()
        {
            var uriPoint = $"{BkrsBaseUri}{_charackter}";            
            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            var document = await context.OpenAsync(uriPoint);
            return document;
        }
    }
}
