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
            int id = 0;
            try
            {
                database_.BeginTransaction();

                string sSQL = "insert into maps (`name`,`createdon`,`access`,`category`,`description`) values (@name, NOW(), 'public', 1, @descr)";
                database_.ExecuteSQL(sSQL,
                        new KeyValuePair<string, object>("@name", name),
                        new KeyValuePair<string, object>("@descr", description)
                        );

                id = (int)database_.ExecuteScalar("select id from maps order by id desc limit 1");

                sSQL = "insert into `users_maps` (`user_id`,`map_id`,`relationship`,`createdon`) values (" + userId + "," + id + ",'owner',NOW())";
                database_.ExecuteSQL(sSQL);

                database_.Commit();
            }
            catch
            {
                database_.RollBack();
            }
            return id;
        }

        public void Delete(int mapId)
        {
            database_.ExecuteSQL("delete from `maps` where id = " + mapId);
            // todo --> check the FK to perform a cascade delete
            database_.ExecuteSQL("delete from `users_maps` where `map_id` = " + mapId);
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