using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;

namespace MVCproject
{
    public class FileCache: ICache, IDisposable
    {

        private string cachePath_;

        public FileCache(string cache_path)
        {
            cachePath_ = cache_path;
        }

        public void Add(ICacheObject cacheObject)
        {
            System.IO.File.WriteAllText(cachePath_ + cacheObject.Id + ".cache", cacheObject.CachedObject.ToString(), Encoding.UTF8);

            if (cacheObject.Expiration != null)
            {
                System.IO.File.WriteAllText(cachePath_ + cacheObject.Id + ".exp", ((DateTime)cacheObject.Expiration).ToString("yyyy|MM|dd|HH|mm|ss"), Encoding.UTF8);
            }
        }

        public string Get(string Id)
        {
            string file = cachePath_ + Id;
            if (System.IO.File.Exists(file + ".cache"))
            {
                if (isExpired(file))
                {
                    return null;
                }

                return System.IO.File.ReadAllText(file + ".cache");
            }
            else
            {
                return null;
            }
        }

        public T Get<T>(string Id)
        {

            string file = cachePath_ + Id;

            if (System.IO.File.Exists(file + ".cache"))
            {
                if (isExpired(file))
                {
                    return default(T);
                }

                using (Stream stream = File.Open(file + ".cache", FileMode.Open))
                {
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    T o = (T)bFormatter.Deserialize(stream);

                    return o;
                }

            }
            else
            {
                return default(T);
            }
        }

        public void Remove(string Id)
        {
            string file = cachePath_ + Id;
            if (System.IO.File.Exists(file + ".cache"))
            {
                try
                {
                    System.IO.File.Delete(file + ".cache");
                }
                catch { }
            }
            if (System.IO.File.Exists(file + ".exp"))
            {
                try
                {
                    System.IO.File.Delete(file + ".exp");
                }
                catch { }
            }
        }

        public void Dispose()
        {
            //so far there is no need to dispose anything...
        }

        private bool isExpired(string file)
        {
            if (System.IO.File.Exists(file + ".exp"))
            {
                string[] expData = System.IO.File.ReadAllText(file + ".exp").Split('|');
                DateTime expDate = new DateTime(int.Parse(expData[0]), int.Parse(expData[1]), int.Parse(expData[2]), int.Parse(expData[3]), int.Parse(expData[4]), int.Parse(expData[5]));

                if (DateTime.Now > expDate)
                {
                    try
                    {
                        System.IO.File.Delete(file + ".exp");
                    }
                    catch { }
                    try
                    {
                        System.IO.File.Delete(file + ".cache");
                    }
                    catch { }
                    return true;
                }
            }
            return false;
        }
    }

    [Serializable]
    public class FileCacheObject : ICacheObject
    {
        public string Id { get; set; }

        public object CachedObject { get; set; }

        public DateTime? Expiration { get; set; }

        public FileCacheObject(string Id, object CachedObject, DateTime? Expiration)
        {
            this.Id = Id;

            if (CachedObject is String) {

                this.CachedObject = CachedObject;

            } else {

            //convert the object into a byte array
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, CachedObject);
                this.CachedObject = ms.ToArray();
            }

            }
            this.Expiration = Expiration;
        }

    }
}