using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingsYear.Infrastructure;
using Microsoft.Extensions.Configuration;
using BuildingsYear.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BuildingsYear.Models.JSONModels;

namespace BuildingsYear.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private MbTilesReader _tileReader;
        private readonly IOptions<List<JsonLayer>> _layersList;

        public MapController(IOptions<List<JsonLayer>> layersList, MbTilesReader tileReader)
        {
            _tileReader = tileReader;
            _layersList = layersList;
        }

        /// <summary>
        /// SYNC getting tile
        /// </summary>
        //[HttpGet("{z}/{x}/{y}.png")]
        //public IActionResult Get(int x, int y, int z)
        //{
        //    var xyz_y = (int)Math.Pow(2, z) - y - 1;
        //    byte[] imageData = _tileReader.GetImageData(x, xyz_y, z);
        //    if (imageData is null)
        //        return null;
        //    return File(imageData, "image/png");
        //}


        /// <summary>
        /// ASYNC getting tile
        /// </summary>
        [HttpGet("{z}/{x}/{y}.png")]
        public async Task<IActionResult> Get(int x, int y, int z)
        {
            return File(await Task.Run(() =>
            {
                var xyz_y = (int)Math.Pow(2, z) - y - 1;
                byte[] imageData = _tileReader.GetImageData(x, xyz_y, z);
                if (imageData is null)
                    return null;
                return imageData;

            }), "image/png");
        }

        [HttpGet("getinfo/{x}/{y}")]
        public JsonResult GetInfo(double x, double y)
        {
            string featureInfo;
            var layer = _layersList.Value.FirstOrDefault(o => o.Name == "const_poly_year_wgs");
            if (layer != null)
            {
                using (GisAccess gis = new GisAccess(layer))
                {
                    featureInfo = gis.GetObjectByCoordinates(x, y);
                }
                if (featureInfo == null)
                {
                    return new JsonResult(new { error = true });
                }
                return new JsonResult(new { featureInfo = featureInfo });
            }
            return new JsonResult(new { error = true, message = "Layer not found" });
        }
    }
}
