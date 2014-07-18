using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MVCproject
{
    public interface ICacheObject
    {

        string Id { get; set; }

        int DatasetId { get; set; }

        object CachedObject {get; set;}

        DateTime? Expiration {get; set;}

    }

    public interface ICache
    {


        void Add(ICacheObject cacheObject);

        string Get(string Id);

        T Get<T>(string Id);

        void Remove(string Id);

        void Remove(int DatasetId);

    }
}
