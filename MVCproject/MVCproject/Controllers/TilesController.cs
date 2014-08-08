using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCproject.Controllers
{
    public class TilesController : BaseController
    {

        // cache the tile in the client
        [OutputCache(Duration = 360000, Location = System.Web.UI.OutputCacheLocation.Client, VaryByParam = "x,y,z,id")]
        public FileContentResult Tile(int x, int y, int z, int id, int map)
        {

            geoTile tile = new geoTile(x, y, z);
            int size = 10;
            Color borderColor, fillColor;
            DataTable points;
            string latCol, lngCol;

            using (Dataset dataset = new Dataset(database))
            {
                // find the dataset with the style for the specified map
                MapDataset ds = dataset.Find(id, map);
                size = ds.style.size;
                borderColor = System.Drawing.ColorTranslator.FromHtml(ds.style.color1);
                fillColor = System.Drawing.ColorTranslator.FromHtml(ds.style.color2);

                var bounds = tile.getBounds(1);

                latCol = ds.latColumn;
                lngCol = ds.lngColumn;

                // retrieve the points for the selected tile only
                points = dataset.FindPoints(ds.tmpTable, ds.latColumn, ds.lngColumn, bounds.minLat, bounds.maxLat, bounds.minLng, bounds.maxLng);
            }

            var geoPoints = new geoTile.geo[points.Rows.Count];
            var cnt = 0;
            foreach (DataRow row in points.Rows)
            {
                geoPoints[cnt] = new geoTile.geo(Convert.ToDouble(row[1]), Convert.ToDouble(row[2]));

                cnt++;
            }

            tile.setPoints(ref geoPoints);

            var bytes = tile.renderTile(borderColor, fillColor, size, 255);

            return new FileContentResult(bytes, "image/png");
        }

    }
}