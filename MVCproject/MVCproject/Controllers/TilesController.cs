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

        public FileContentResult Tile(int x, int y, int z, int id)
        {

            geoTile tile = new geoTile(x, y, z);
            DataTable points;
            string latCol, lngCol;

            using (Dataset dataset = new Dataset(database))
            {
                MapDataset ds = dataset.Find(id);

                latCol = ds.latColumn;
                lngCol = ds.lngColumn;

                // retrieve the points for the selected tile only
                points = dataset.FindPoints(ds.tmpTable, ds.latColumn, ds.lngColumn, tile.bounds.minLat, tile.bounds.maxLat, tile.bounds.minLng, tile.bounds.maxLng);
            }

            var geoPoints = new geoTile.geo[points.Rows.Count];
            var cnt = 0;
            foreach (DataRow row in points.Rows)
            {
                geoPoints[cnt] = new geoTile.geo(Convert.ToDouble(row[1]), Convert.ToDouble(row[2]));

                cnt++;
            }

            tile.setPoints(geoPoints);

            var bytes = tile.renderTile(Color.Yellow, Color.Red, 11, 255);

            return new FileContentResult(bytes, "image/png");
        }

    }
}
