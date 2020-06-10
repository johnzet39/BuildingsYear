using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BuildingsYear.Models;
using BuildingsYear.Models.JSONModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Threading;

namespace BuildingsYear.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IConfiguration _configuration;
        private readonly IOptions<List<JsonLayer>> _layersList;

        public HomeController(ILogger<HomeController> logger,
                                     IConfiguration configuration,
                                     IOptions<List<JsonLayer>> layersList)
        { 
            _configuration = configuration;
            _logger = logger;
            _layersList = layersList;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EditBuilding(int keyid)
        {
            return PartialView("EditBuildingModal", new UserBuilding { Keyid = keyid} );
        }

        [HttpPost]
        public IActionResult EditBuilding(UserBuilding userbuilding)
        {

            if (ModelState.IsValid)
            {
                var usersTable = _layersList.Value.FirstOrDefault(o => o.Name == "users_data");
                if (usersTable != null)
                {
                    using (GisAccess gis = new GisAccess(usersTable))
                    {
                        int retry_cnt = 5;
                        int cnt = retry_cnt;
                        do
                        {
                            --cnt;
                            try
                            {
                                gis.WriteUserData(userbuilding);
                                break;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Not write userdata {retry_cnt - cnt}/{retry_cnt}. {ex.Message}");
                                Thread.Sleep(2);
                            }
                        } while (cnt > 0);
                    }
                }
            }

            return PartialView("EditBuildingModal", userbuilding);
            
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
