using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SeoTest.Models
{
    public class SeoSearchModel
    {
        public IEnumerable<SelectListItem> SearchEngines { get; set; }
        
        public string SearchEngine { get; set; }        
        
        public string Keywords { get; set; }

        public string MatchUrl { get; set; }
        public int MatchPosition { get; set; }
        public DateTime SearchTime { get; set; }
    }
}