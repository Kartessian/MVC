using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCproject
{
    public class Maps: IDisposable
    {
        private Database database_;

        public Maps(Database database)
        {
            this.database_ = database;
        }

        public int Create(string name, string description, int userId)
        {
            return 0;
        }

        public void Delete(int mapId)
        {
        }

        public void Update(int mapId, string newName, string newDescription)
        {
        }

        public void Update(int mapId, string newStyle)
        {
        }

        public List<UserMaps> UserMaps(int userId)
        {
            return database_.GetRecords<UserMaps>(
                "select m.* from maps m inner join users_maps u on u.map_id = m.id and u.user_id = @userId ",
                new KeyValuePair<string, object>("@userId", userId)
            );
        }

        public void Dispose()
        {
            database_ = null;
        }
    }
}