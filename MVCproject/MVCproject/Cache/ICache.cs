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

        string ID {get; set;}

        object CachedObject {get; set;}

        DateTime? Expiration {get; set;}

    }

    public interface ICache
    {


        void Add(ICacheObject cacheObject);

        string Get(string ID);

        T Get<T>(string ID);

        void Remove(string ID);


    }
}
