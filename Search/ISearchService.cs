using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeoTest.Search
{
    public interface ISearchService
    {
        List<string> GetSearchEngines();

        Task<(int postion, DateTime time)> Search(string engine, string keywords, string matchUrl);
    }
}