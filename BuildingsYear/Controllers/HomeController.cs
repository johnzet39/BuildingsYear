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
using BuildingsYear.Infrastructure;
using Microsoft.AspNetCore.Http;

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
            if (EditesChecker.IsMaxEditesPerDay())
                return PartialView("EditBuildingModal", new UserBuilding { Keyid = keyid} );
            else {
                _logger.LogWarning($"Превышен лимит правок в день. ({HttpContext.Connection.RemoteIpAddress.ToString()})");
                return StatusCode(StatusCodes.Status405MethodNotAllowed, "Превышен лимит правок в день.");
            }
            
        }

        [HttpPost]
        public IActionResult EditBuilding(UserBuilding userbuilding)
        {
            
            if (ModelState.IsValid)
            {
                userbuilding.IpAddressUser = HttpContext.Connection.RemoteIpAddress.ToString();
                var usersTable = _layersList.Value.FirstOrDefault(o => o.Name == "users_data");
                if (usersTable != null)
                {
                    using (GisAccess gis = new GisAccess(usersTable))
                    {
                        //if (gis.IsMaxEditesPerDay())
                        //{
                        int retry_cnt = 5;
                        int cnt = retry_cnt;
                        do
                        {
                            --cnt;
                            try
                            {
                                gis.WriteUserData(userbuilding);
                                EditesChecker.AddEditsCnt();
                                break;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Not write userdata {retry_cnt - cnt}/{retry_cnt}. {ex.Message}");
                                Thread.Sleep(2);
                            }
                        } while (cnt > 0);
                        //}
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
