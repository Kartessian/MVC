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

        public DataTable GetPoint(string tmpTable, int point)
        {
            return database_.GetDataTable("select * from `datasets`.`" + tmpTable + "`", new KeyValuePair<string, object>("@id", point));
        }

        public void Delete(int DatasetId)
        {
            object existing_table_name = database_.ExecuteScalar("select tableName from datasets where id = @id",
                new KeyValuePair<string, object>("@id", DatasetId));

            if (existing_table_name != null && existing_table_name != DBNull.Value)
            {
                // remove the entry from the datasets table
                database_.ExecuteSQL("delete from `datasets` where name = @id", new KeyValuePair<string, object>("@id", DatasetId));

                // drop the table asocciated to the dataset
                database_.ExecuteSQL("drop table `datasets`.`" + existing_table_name + "`");
            }
        }

        public void Dispose()
        {
            database_ = null;
        }
    }
}