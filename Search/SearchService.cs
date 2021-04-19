using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SeoTest.Models;

namespace SeoTest.Search
{
    public class SearchService : ISearchService
    {
        private readonly IOptions<SeoSettings> _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;

        public SearchService(IOptions<SeoSettings> options, IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _options = options;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        public List<string> GetSearchEngines()
        {
            return _options.Value.Items.Select(k => k.SearchEngine).ToList();
        }

        public async Task<(int postion, DateTime time)> Search(string engine, string keywords, string matchUrl)
        {
            return await _cache.GetOrCreateAsync<(int postion, DateTime time)>($"{engine}.{keywords}.{matchUrl}", async cacheEntry =>
            {
                var setting = _options.Value.Items.SingleOrDefault(k => k.SearchEngine == engine);
                if (setting == null)
                {
                    cacheEntry.SetAbsoluteExpiration(TimeSpan.FromHours(1));
                    return (0, DateTime.Now);
                }

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(setting.BaseUrl);
                var searchQueryFormat = setting.QueryFormat;
                var searchQuery = searchQueryFormat.Replace("{keywords}", Uri.EscapeUriString(keywords));
                var result = await Search(client, searchQuery, matchUrl, setting.MaxResults, setting.OuterRepeatingElement);
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromHours(1));
                return (result, DateTime.Now);
            });
        }

        private async Task<int> Search(HttpClient client, string searchQuery, string matchUrl, int maxResults, string OuterRepeatingElement)
        {
            if (maxResults <= 0) return -1;

            var itemNo = 0;
            while (itemNo <= maxResults)
            {
                var response = await client.GetStringAsync(searchQuery.Replace("{itemNo}", itemNo.ToString()));
                var outerTags = GetMatchingOuterHtmlTags(response, matchUrl, OuterRepeatingElement);
                if (outerTags == null)
                {
                    itemNo += 10;
                    continue;
                }
                var urls = GetUrls(response, outerTags.Value);
                var position = urls.FindIndex(url => url.Contains(matchUrl));
                return itemNo + position + 1;
            }
            return -1;
        }

        private List<string> GetUrls(string response, (string openTag, string closeTag) tags)
        {
            var urls = new List<string>();
            var index = 0;
            while (true)
            {
                var startIndex = response.IndexOf(tags.openTag, index);
                if (startIndex < 0) break;
                var endIndex = response.IndexOf(tags.closeTag, startIndex);
                urls.Add(response.Substring(startIndex + tags.openTag.Length, endIndex - startIndex - tags.openTag.Length));
                index = endIndex + tags.closeTag.Length;
            }
            return urls;
        }

        private (string openTag, string closeTag)? GetMatchingOuterHtmlTags(string response, string matchUrl, string outerRepeatingElement)
        {
            if (!matchUrl.StartsWith("https://"))
                matchUrl = "https://" + matchUrl;
            var openTagStartIndex = response.IndexOf(outerRepeatingElement + matchUrl);
            if (openTagStartIndex < 0) return null;
            var tag = response.Substring(openTagStartIndex + 1, response.IndexOf(" ", openTagStartIndex) - openTagStartIndex - 1);
            var openTagEndIndex = openTagStartIndex + outerRepeatingElement.Length;
            return (response.Substring(openTagStartIndex, openTagEndIndex - openTagStartIndex), $"</{tag}>");
        }
    }
}