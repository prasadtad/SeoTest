using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SeoTest.Models;
using SeoTest.Search;

namespace SeoTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISearchService _service;

        public HomeController(ISearchService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(SeoSearchModel model)
        {
            if (model == null)
            {
                model = new SeoSearchModel
                {
                    SearchEngines = _service.GetSearchEngines().Select(s => new SelectListItem(s, s)),
                    MatchPosition = 0
                };
            }
            else
            {
                var searchResult = await _service.Search(model.SearchEngine, model.Keywords, model.MatchUrl);
                model.SearchEngines = _service.GetSearchEngines().Select(s => new SelectListItem(s, s));
                model.MatchPosition = searchResult.postion;
                model.SearchTime = searchResult.time;
            }
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
