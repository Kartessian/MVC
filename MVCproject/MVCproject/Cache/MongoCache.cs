using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using MongoDB.Driver.Builders;


namespace MVCproject
{
    public class MongoCache : ICache
    {
        private MongoClient mongo_;
        private MongoServer server_;
        private MongoDatabase db_;
        private MongoCollection<MongoCacheObject> collection_;

        public MongoCache()
        {
            mongo_ = new MongoClient("mongodb://localhost");
            server_ = mongo_.GetServer();
            db_ = server_.GetDatabase("cache");

            collection_ = db_.GetCollection<MongoCacheObject>("cachedObject");
        }

        public void Add(ICacheObject cacheObject)
        {
            collection_.Insert((MongoCacheObject)cacheObject);
        }

        /// <summary>
        ///  will only return a value when the cached object is a string
        /// </summary>
        public string Get(string Id)
        {
            var cached = collection_.FindOne(Query<MongoCacheObject>.EQ(Q => Q.Id, Id));
            if (cached != null)
            {
                if (cached.Expiration >= DateTime.Now && cached.CachedObject is string)
                {
                    return (string)cached.CachedObject;
                }
                else
                {
                    //remove from the collection as is expired
                    collection_.Remove(Query<MongoCacheObject>.EQ(Q => Q.Id, Id));
                }
            }
            return null;
        }

        public T Get<T>(string Id)
        {
            var cached = collection_.FindOne(Query<MongoCacheObject>.EQ(Q => Q.Id, Id));
            if (cached != null)
            {
                if (cached.Expiration >= DateTime.Now)
                {
                    return (T)cached.CachedObject;
                }
                else
                {
                    //remove from the collection as is expired
                    collection_.Remove(Query<MongoCacheObject>.EQ(Q => Q.Id, Id));
                }
            }
            return default(T);
        }

        public void Remove(string Id)
        {
            collection_.Remove(Query<MongoCacheObject>.EQ(Q => Q.Id, Id));
        }

        public void Remove(int DatasetId)
        {
            collection_.Remove(Query<MongoCacheObject>.EQ(Q => Q.DatasetId, DatasetId));
        }

        public void Dispose()
        {
            // Mongo driver takes care of the connection, so won't really be needed to do anything here...
            GC.SuppressFinalize(this);
        }
    }

    public class MongoCacheObject : ICacheObject
    {
        public string Id {get; set;}

        public int DatasetId { get; set; }

        public object CachedObject { get; set; }

        public DateTime? Expiration { get; set; }

        public MongoCacheObject(string Id, object cachedObject, DateTime expiration)
        {
            this.Id = Id;
            this.CachedObject = cachedObject;
            this.Expiration = expiration;
        }
    }
}