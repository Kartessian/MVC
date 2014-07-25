using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCproject
{

    public class Dataset : IDisposable
    {
        private Database database_;

        public Dataset(Database database)
        {
            this.database_ = database;
        }

        /// <summary>
        /// Creates a new dataset for the specified user, using the data in the datatable
        /// </summary>
        public int CreateFromTable(DataTable table, int userId, string fileName)
        {

            // this is usually an intermediate step, so the user could never finish the process
            // will check if it has any pending table from the last attempt and delete it before continue
            object existing_table_name = database_.ExecuteScalar("select tableName from datasets where name = @name", 
                new KeyValuePair<string, object>("@name", userId + "temp-Name"));

            if (existing_table_name != null && existing_table_name != DBNull.Value)
            {
                // remove the entry from the datasets table
                database_.ExecuteSQL("delete from `datasets` where name = @name", new KeyValuePair<string, object>("@name", userId + "temp-Name"));

                // drop the table asocciated to the dataset
                database_.ExecuteSQL("drop table `datasets`.`" + existing_table_name + "`");

                // can use the same table name, no need to generate a new one
                table.TableName = (string)existing_table_name;
            }
            else
            {

                // assign a new unique name to the table
                // need to be sure there is no table with that name already in the database
                bool nameExist = true;
                while (nameExist)
                {
                    table.TableName = Helpers.GenerateUniqueName();
                    nameExist = database_.ExistTable(table.TableName, "datasets");
                }
            }

            // all the data for the Datasets are located in its own schema "datasets" in the database, to keep 
            // it separated from the application tables
            if (database_.InsertDataTable(table, "datasets"))
            {
                // once the table is created, add an entry to the "datasets" table
                string sSQL = "insert into datasets (`name`,`createdon`,`access`,`filename`,`tmptable`,`rows`) values (@name,NOW(),'private',@filename,@tablename,@rows)";
                database_.ExecuteSQL(sSQL,
                        new KeyValuePair<string, object>("@name", userId + "temp-Name"),
                        new KeyValuePair<string, object>("@filename", fileName),
                        new KeyValuePair<string, object>("@tablename", table.TableName),
                        new KeyValuePair<string, object>("@rows", table.Rows.Count)
                    );

                // get the id of this dataset
                sSQL = "select id from `datasets` where `name` = @name";
                return (int)database_.ExecuteScalar(sSQL, new KeyValuePair<string, object>("@name", userId + "temp-Name"));

            }

            return 0;
        }

        /// <summary>
        /// Add a dataset to the specified map
        /// </summary>
        public void Attach(int DatasetId, int mapId)
        {
        }

        /// <summary>
        /// Deletes the relationship between a map and a dataset
        /// </summary>
        public void Detach(int DatasetId, int mapId)
        {

        }

        /// <summary>
        /// Deletes the specified dataset. It will also drop the table that contains the data
        /// </summary>
        public void Delete(int DatasetId)
        {
            object existing_table_name = database_.ExecuteScalar("select tableName from datasets where id = @id",
                new KeyValuePair<string, object>("@id", DatasetId));

            if (existing_table_name != null && existing_table_name != DBNull.Value)
            {
                database_.BeginTransaction();

                try
                {
                    // remove the entry from the datasets table
                    database_.ExecuteSQL("delete from `datasets` where name = @id", new KeyValuePair<string, object>("@id", DatasetId));

                    // drop the table asocciated to the dataset
                    database_.ExecuteSQL("drop table `datasets`.`" + existing_table_name + "`");

                    database_.Commit();

                } catch {
                    database_.RollBack();
                }
            }
        }

        public long AddPoint(string TableName, string latColumn, string lngColumn, double latitude, double longitude)
        {
            database_.BeginTransaction();
            try
            {
                database_.ExecuteSQL("insert into `datasets`.`" + TableName + "` (`" + latColumn + "`,`" + lngColumn + "`) values (@lat,@lng)",
                        new KeyValuePair<string, object>("@lat", latitude),
                        new KeyValuePair<string, object>("@lng", longitude)
                    );

                long newId = (long)database_.ExecuteScalar("select id from `datasets`.`" + TableName + "` order by id desc limit 1");

                database_.Commit();

                return newId;
            }
            catch
            {
                database_.RollBack();
                return 0;
            }
        }

        public void DeletePoint(string TableName, int PointId)
        {
            database_.ExecuteSQL("delete from `datasets`.`" + TableName + "` where id = @id",
                    new KeyValuePair<string, object>("@id", PointId)
                );
        }

        public void Update(int DatasetId, string newName)
        {
            database_.ExecuteSQL(
                "update datasets set name = @name where id = @id",
                new KeyValuePair<string,object>("@name", newName),
                new KeyValuePair<string, object>("@id", DatasetId)
                );
        }

        /// <summary>
        /// Returns a list with all public available datasets
        /// </summary>
        public List<MapDataset> PublicList()
        {
            return database_.GetRecords<MapDataset>(new KeyValuePair<string, object>("access", "public"));
        }

        /// <summary>
        /// Return a list with all the datasets that belongs or are available 
        /// (because they are public and the user added to one of his maps) to a specified user
        /// </summary>
        public List<MapDataset> UserList(int userId)
        {
            return database_.GetRecords<MapDataset>(
                "select d.* from datasets d inner join users_datasets u on u.dataset_id = d.id and u.user_id = @userId", 
                new KeyValuePair<string, object>("@userId", userId)
            );
        }

        /// <summary>
        /// Return a list with the datasets currently being used by an user on his maps
        /// </summary>
        public List<MapDataset> UserActiveList(int userId)
        {
            return database_.GetRecords<MapDataset>(
                "select d.* from datasets d " +
                " inner join users_datasets u on u.dataset_id = d.id and u.user_id = @userId " +
                " inner join maps_datasets m on m.dataset_id = d.id", 
                new KeyValuePair<string, object>("@userId", userId)
            );
        }

        /// <summary>
        /// Returns the row for the selected point in the dataset table
        /// </summary>
        public DataTable GetPoint(string tmpTable, int point)
        {
            return database_.GetDataTable("select * from `datasets`.`" + tmpTable + "` where id = @id", new KeyValuePair<string, object>("@id", point));
        }

        public void Dispose()
        {
            database_ = null;
        }
    }
}