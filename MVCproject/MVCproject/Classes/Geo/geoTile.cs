using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace MVCproject
{
    public class geoTile
    {
        const int BlueOffset = 0;
        const int GreenOffset = 1;
        const int RedOffset = 2;
        const int AlphaOffset = 3;

        public class boundaries
        {
            public double minLat;
            public double maxLat;
            public double minLng;
            public double maxLng;

            public boundaries(double minlat, double maxlat, double minlng, double maxlng)
            {
                this.minLat = minlat;
                this.maxLat = maxlat;
                this.minLng = minlng;
                this.maxLng = maxlng;
            }
        }

        public class geo
        {
            public double lat;
            public double lng;

            public geo(double lat, double lng)
            {
                this.lat = lat;
                this.lng = lng;
            }
        }

        public Int32 mapSize;
        public Int32 tileSize;
        public int X;
        public int Y;
        private int zoom;
        public int Zoom
        {
            get
            {
                return zoom;
            }
            set
            {
                zoom = value;
                mapSize = 256 << zoom;
                tileSize = Convert.ToInt32(mapSize / Math.Pow(2, zoom));
            }
        }

        public Point topLeft;
        public Point bottomRight;

        private geo[] points;

        public boundaries bounds;

        public geoTile(int x, int y, int zoom)
        {
            this.X = x;
            this.Y = y;
            this.zoom = zoom;
            this.mapSize = 256 << zoom;
            this.tileSize = Convert.ToInt32(mapSize / Math.Pow(2, zoom));
            this.topLeft = new Point(x * tileSize, y * tileSize);
            this.bottomRight = new Point(x * tileSize + tileSize, y * tileSize + tileSize);

            bounds = getBounds(0);
        }

        public void setPoints(ref geo[] points)
        {
            this.points = points;
        }

        public boundaries getBounds(double adjust = 0)
        {

            double latitude1, latitude2, longitude1, longitude2;

            int pixelX = this.X * tileSize;
            int pixelY = this.Y * tileSize;

            double x = (Clip(pixelX, 0, mapSize - 1) / mapSize) - 0.5;
            double y = 0.5 - (Clip(pixelY, 0, mapSize - 1) / mapSize);

            latitude1 = 90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI + adjust;
            longitude1 = 360 * x - adjust;

            pixelX = (this.X + 1) * tileSize - 1;
            pixelY = (this.Y + 1) * tileSize - 1;

            x = (Clip(pixelX, 0, mapSize - 1) / mapSize) - 0.5;
            y = 0.5 - (Clip(pixelY, 0, mapSize - 1) / mapSize);

            latitude2 = 90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI - adjust;
            longitude2 = 360 * x + adjust;

            double aux;
            if (latitude1 > latitude2)
            {
                aux = latitude1;
                latitude1 = latitude2;
                latitude2 = aux;
            }
            if (longitude1 > longitude2)
            {
                aux = longitude1;
                longitude1 = longitude2;
                longitude2 = aux;
            }

            return new boundaries(latitude1, latitude2, longitude1, longitude2);
        }

        public geo getLatLngfromXY(int x, int y)
        {
            double xDiff = Math.Abs(Math.Abs(bounds.minLng) - Math.Abs(bounds.maxLng)) / 256;
            double yDiff = Math.Abs(Math.Abs(bounds.minLat) - Math.Abs(bounds.maxLat)) / 256;

            return new geo(bounds.minLng + x * xDiff, bounds.minLat - y * yDiff);
        }

        public Point getXYfromLatLng(geo coordinates)
        {
            return getXYfromLatLng(coordinates.lat, coordinates.lng);
        }

        public Point getXYfromLatLng(double latitude, double longitude)
        {
            double x = (longitude + 180) / 360;
            double sinLatitude = Math.Sin(latitude * Math.PI / 180);
            double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);


            return new Point((int)Clip(x * mapSize + 0.5, 0, mapSize - 1) - X * tileSize,
                (int)Clip(y * mapSize + 0.5, 0, mapSize - 1) - Y * tileSize);
        }

        public byte[] renderTile(Color borderColor, Color fillColor, int pointSize, int alpha)
        {
            using (System.Drawing.Bitmap bitmap = new Bitmap(256, 256))
            {
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                var bytes = new byte[bitmapData.Stride * bitmap.Height];
                Marshal.Copy(bitmapData.Scan0, bytes, 0, bytes.Length);
                bitmap.UnlockBits(bitmapData);

                int height = bitmap.Height;
                int width = bitmap.Width;
                int stride = bitmapData.Stride;
                int bytesPerPixel = bitmapData.Stride / bitmap.Width;
                int offsetlength = bytes.Length;

                // no longer need the data
                bitmapData = null;

                int minY, maxY, minX, maxX;

                double distance = pointSize * pointSize;

                Color color = fillColor;

                for (int i = 0, len = points.Length; i < len; i++)
                {
                    Point point = getXYfromLatLng(points[i].lat, points[i].lng);


                    minY = point.Y - pointSize;
                    if (minY < 0) minY = 0;

                    maxY = point.Y + pointSize;
                    if (maxY >= height) maxY = height - 1;

                    minX = point.X - pointSize;
                    if (minX < 0) minX = 0;

                    maxX = point.X + pointSize;
                    if (maxX >= width) maxX = width - 1;

                    for (int y = minY; y <= maxY; y++)
                    {
                        for (int x = minX; x <= maxX; x++)
                        {
                            if (distance >= (point.X - x) * (point.X - x) + (point.Y - y) * (point.Y - y))
                            {

                                // locate the pixel in the byte array
                                int offset = x * bytesPerPixel + y * stride;

                                // set the color value for it
                                bytes[offset] = color.B;
                                bytes[offset + GreenOffset] = color.G;
                                bytes[offset + RedOffset] = color.R;
                                bytes[offset + AlphaOffset] = color.A;
                            }
                        }
                    }
                }


                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bytes, 0, bitmapData.Scan0, offsetlength);
                bitmap.UnlockBits(bitmapData);
                bitmapData = null;

                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                return stream.ToArray();
            }
        }

        private double Clip(double n, double minValue, double maxValue)
        {
            return Math.Min(Math.Max(n, minValue), maxValue);
        }
    }
}