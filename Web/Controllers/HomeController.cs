using System.Diagnostics;
using System.Threading.Tasks;
using RoyalFamily.Common;
using RoyalFamily.Common.Services;
using RoyalFamily.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace RoyalFamily.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPersonService _personService;

        public HomeController(IPersonService personService)
        {
            _personService = personService;
        }

        [HttpGet]
        public IActionResult Index()
        {         
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RelationshipViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var relationship = await _personService.GetRelationship(model.Name1, model.Name2);
                    model.Relationship = relationship;
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }                
            }
            return View(model);
        }

        public IActionResult Error()
        {
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
