using System;
using OSGeo.OGR;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddAgree.Models;
using Newtonsoft.Json;
using System.Dynamic;

namespace BuildingsYear.Models
{
    public class GisAccess : IDisposable
    {
        private const int OGRERR_NONE = 0;
        private const int GEOM_FIELD_IDX = 0;

        private NumberFormatInfo nfi = new NumberFormatInfo() { CurrencyDecimalSeparator = "." };

        protected bool disposed = false;
        protected DataSource ds;
        protected Layer pglayer;
        private JSONModels.JsonLayer _jsonLayer;


        public GisAccess(JSONModels.JsonLayer jsonLayer)
        {
            _jsonLayer = jsonLayer;
            string connstring = jsonLayer.DataSource;

            Ogr.RegisterAll();

            var driver = Ogr.GetDriverByName(jsonLayer.Provider);

            ds = driver.Open(connstring, 1);

            if (ds == null)
            {
                throw new GisException("DataSource object is null");
            }

            if (ds.GetLayerCount() == 0)
            {
                ds.Dispose();
                ds = null;
                throw new GisException("Datasource has no layers");
            }

            pglayer = ds.GetLayerByName(jsonLayer.Name);
        }

        public string GetObjectByCoordinates(double x, double y)
        {
            //x = Normalize(x);
            string wkt = $"POINT({x.ToString(nfi)} {y.ToString(nfi)})";
            Geometry filter = null;
            Feature feature = null;
            try
            {
                filter = Ogr.CreateGeometryFromWkt(ref wkt, pglayer.GetLayerDefn().GetGeomFieldDefn(GEOM_FIELD_IDX).GetSpatialRef());
                pglayer.SetSpatialFilter(filter);
                feature = pglayer.GetNextFeature();

                if (feature != null)
                {
                    string featureWkt = null;
                    int error = feature.GetGeomFieldRef(GEOM_FIELD_IDX).ExportToWkt(out featureWkt);
                    if (error == OGRERR_NONE)
                    {
                        var jsonobject = new
                        {
                            year = feature.GetFieldAsString("year"),
                            address = feature.GetFieldAsString("address_in"),
                            keyid = feature.GetFieldAsInteger("KEYID"),
                            klgd_descr = feature.GetFieldAsString("klgd_gallery_descr"),
                            klgd_img_url = feature.GetFieldAsString("klgd_gallery_img_url"),
                            klgd_source = feature.GetFieldAsString("klgd_source_info"),
                            geom = featureWkt
                        };
                        return Newtonsoft.Json.JsonConvert.SerializeObject(jsonobject);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new GisException("Error getting feature by coordinates", ex);
            }
            finally
            {
                if (filter != null)
                {
                    filter.Dispose();
                }
                if (feature != null)
                {
                    feature.Dispose();
                }
            }
            return null;
        }

        #region get geojson
        public string GetAsGeoJson()
        {
            pglayer.ResetReading();
            JSONModels.RootObject gjo = new JSONModels.RootObject
            {
                type = "FeatureCollection",
                name = pglayer.GetName(),
                features = GetFeatures(pglayer)
            };
            return JsonConvert.SerializeObject(gjo);
        }

        private List<JSONModels.Feature> GetFeatures(Layer layer)
        {
            Feature f;
            List<JSONModels.Feature> features = new List<JSONModels.Feature>();
            while ((f = layer.GetNextFeature()) != null)
            {
                JSONModels.Feature feature = new JSONModels.Feature
                {
                    type = "Feature",
                    properties = GetProperties(f),
                    geometry = GetGeometry(f)
                };
                features.Add(feature);
            }
            return features;
        }

        private IDictionary<string, object> GetProperties(Feature f)
        {
            string[] available_attributes = _jsonLayer.Attributes; //list of attributes from appsettings for every layer
            var Props = new ExpandoObject() as IDictionary<string, Object>;
            Props.Add("fid", Convert.ToInt32(f.GetFID()));
            foreach (var field_idx in Enumerable.Range(0, f.GetFieldCount()))
            {
                string field_name = f.GetFieldDefnRef(field_idx).GetName();
                if (available_attributes.Contains(field_name, StringComparer.OrdinalIgnoreCase))
                {
                    switch (f.GetFieldType(field_idx).ToString())
                    {
                        case "OFTInteger":
                            Props.Add(field_name, f.GetFieldAsInteger(field_idx));
                            break;
                        case "OFTString":
                            Props.Add(field_name, f.GetFieldAsString(field_idx));
                            break;
                        case "OFTDateTime":
                            Props.Add(field_name, f.GetFieldAsString(field_idx));
                            break;
                        case "OFTDouble":
                            Props.Add(field_name, f.GetFieldAsDouble(field_idx));
                            break;
                        case "OFTBoolean":
                            Props.Add(field_name, Convert.ToBoolean(f.GetFieldAsInteger(field_idx)));
                            break;
                        default:
                            break;
                    };
                }
            }
            return Props;
            #region old
            //foreach (var p in Props.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            //{
            //    if (p.Name == "gid" || p.Name == "id")
            //    {
            //        p.SetValue(Props, Convert.ToInt32(f.GetFID()), null);
            //        continue;
            //    };
            //    if (p != null && p.CanWrite)
            //    {
            //        switch (p.PropertyType.ToString())
            //        {
            //            case "System.Int32":
            //                p.SetValue(Props, f.GetFieldAsInteger(p.Name), null);
            //                break;
            //            case "System.String":
            //                p.SetValue(Props, f.GetFieldAsString(p.Name), null);
            //                break;
            //            case "System.Boolean":
            //                p.SetValue(Props, Convert.ToBoolean(f.GetFieldAsInteger(p.Name), null));
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}
            //return Props;
            #endregion
        }

        private dynamic GetGeometry(Feature f)
        {
            JSONModels.Geometry geometry = new JSONModels.Geometry();
            var f_geom = f.GetGeometryRef();
            geometry.type = f_geom.GetGeometryName();

            string f_geom_json = f_geom.ExportToJson(null);
            geometry = JsonConvert.DeserializeObject<JSONModels.Geometry>(f_geom_json);
            return geometry;
        }
        #endregion TEST POLY


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Защищенная реализация метода Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Очистить все управляемые объекты
                if (pglayer != null)
                {
                    pglayer.Dispose();
                    pglayer = null;
                }
                if (ds != null)
                {
                    ds.Dispose();
                    ds = null;
                }
            }

            // Освободить все неуправляемые ресурсы
            // ...
            disposed = true;
        }

        private double Normalize(double x)
        {
            x = x % 360;
            if (x > 180)
            {
                x -= 360;
            }
            else if (x < -180)
            {
                x += 360;
            }
            return x;
        }

        ~GisAccess()
        {
            Dispose(false);
        }
    }
}