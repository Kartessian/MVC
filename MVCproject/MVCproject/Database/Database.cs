using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MVCproject
{
    public class Database: IDisposable
    {
        private MySqlConnection _conn;
        private MySqlTransaction _tran;

        public Database(string connectionString) {

            _conn = new MySqlConnection(connectionString);
            _conn.Open();
        
        }

        public void Dispose()
        {
            if (_conn.State != ConnectionState.Closed)
            {
                _conn.Close();
            }
            _conn.Dispose();
        }

        public void BeginTransaction()
        {
            _tran = _conn.BeginTransaction(IsolationLevel.Serializable);
        }

        public void Commit()
        {
            if (_tran != null)
            {
                _tran.Commit();
                _tran.Dispose();
            }
        }

        public void RollBack()
        {
            if (_tran != null)
            {
                _tran.Rollback();
                _tran.Dispose();
            }
        }

        private MySqlCommand CreateCommand(string query, params KeyValuePair<string, object>[] Parameters)
        {
            MySqlCommand comm;

            if (_tran != null)
                comm = new MySqlCommand(query, _conn, _tran);
            else
                comm = new MySqlCommand(query, _conn);

            if (Parameters != null)
            {
                foreach (KeyValuePair<string, object> pair in Parameters)
                {
                    if (pair.Value == null)
                    {
                        comm.Parameters.AddWithValue(pair.Key, DBNull.Value);
                    }
                    else
                    {
                        comm.Parameters.AddWithValue(pair.Key, pair.Value);
                    }
                }
            }
            return comm;
        }

        public bool ExistTable(string schema, string tableName)
        {
            return 1 == (long)ExecuteScalar("select count(*) from information_schema.tables where table_schema = '" + schema + "' and table_name = '" + tableName + "'");
        }

        public int ExecuteSQL(string query, params KeyValuePair<string, object>[] Parameters)
        {
            MySqlCommand comm = CreateCommand(query, Parameters);

            return comm.ExecuteNonQuery();
        }

        public object ExecuteScalar(string query, params KeyValuePair<string, object>[] Parameters)
        {
            MySqlCommand comm = CreateCommand(query, Parameters);

            return comm.ExecuteScalar();
        }

        public DataTable GetDataTable(string query, params KeyValuePair<string, object>[] Parameters)
        {
            DataTable oDataTable = new DataTable();

            MySqlCommand comm = CreateCommand(query, Parameters);

            MySqlDataAdapter oDataAdapter = new MySqlDataAdapter(comm);
            oDataAdapter.Fill(oDataTable);
            oDataAdapter.Dispose();
            comm.Dispose();

            return oDataTable;
        }

        public List<T> GetRecords<T>(params KeyValuePair<string, object>[] parameters) where T : ITable, new()
        {
            List<T> result = new List<T>();

            T newObject = new T();

            List<string> Properties = GetObjectProperties<T>();

            List<string> where = new List<string>();
            foreach (var param in parameters)
            {
                if (param.Key.StartsWith("@"))
                {
                    where.Add(param.Key.Substring(1) + " = " + param.Key);
                }
                else
                {
                    where.Add(param.Key + " = @" + param.Key);
                }
            }

            string whereCondition = string.Empty;
            if (where.Count > 0)
            {
                whereCondition = " where " + string.Join(" and ", where);
            }

            DataTable dt = GetDataTable("select * from " + newObject.TableName + whereCondition, parameters);
            for (int i = 0; i < Properties.Count; i++)
            {
                if (!dt.Columns.Contains(Properties[i]))
                {
                    Properties.RemoveAt(i);
                    i--;
                }
            }

            foreach (DataRow reader in dt.Rows)
            {
                foreach (string propName in Properties)
                {
                    if (((!object.ReferenceEquals(reader[propName], DBNull.Value))))
                    {
                        newObject.GetType().GetProperty(propName).SetValue(newObject, reader[propName], null);
                    }
                }

                result.Add(newObject);
            }


            return result;
        }

        private List<string> GetObjectProperties<T>() where T : ITable
        {
            List<string> properties = new List<string>();
            foreach (PropertyInfo info in typeof(T).GetProperties())
            {

                if (info.GetCustomAttribute(typeof(RelatedField), false) == null)
                {
                    properties.Add(info.Name);
                }
            }

            return properties;
        }

        public bool InsertDataTable(DataTable dt, string schema = null)
        {
            schema = string.IsNullOrEmpty(schema) ? "" : "`" + schema + "`.";

            string tableName = dt.TableName;

            string sSQL = sSQL = "CREATE TABLE " + schema + "`" + tableName + "` (`id` INT NOT NULL AUTO_INCREMENT ";
            string insertSQL = "insert into " + schema + "`" + tableName + "` (";
            string parametersSQL = "";

            int ix = 0;
            foreach (DataColumn column in dt.Columns)
            {
                if(column.DataType == typeof(string)) {
                    // TODO - find the max length of the current values change the 255 with the value found.
                    sSQL += ", `" + column.ColumnName + "` varchar (255) DEFAULT NULL";
                }
                else if (column.DataType == typeof(DateTime))
                {
                    sSQL += ", `" + column.ColumnName + "` datetime DEFAULT NULL";
                }
                else if (column.DataType == typeof(int))
                {
                    sSQL += ", `" + column.ColumnName + "` int DEFAULT NULL";
                }
                else if (column.DataType == typeof(long))
                {
                    sSQL += ", `" + column.ColumnName + "` bigint DEFAULT NULL";
                }
                else if (column.DataType == typeof(float))
                {
                    sSQL += ", `" + column.ColumnName + "` float DEFAULT NULL";
                }
                else if (column.DataType == typeof(double))
                {
                    sSQL += ", `" + column.ColumnName + "` double DEFAULT NULL";
                }


                if (column.ColumnName.ToLower() == "id")
                {
                    insertSQL += ",`ktsn_" + column.ColumnName + "`";
                }
                else
                {
                    insertSQL += ",`" + column.ColumnName + "`";
                }
                parametersSQL += ",@param" + ix;

                ix++;
            }

            sSQL += ", PRIMARY KEY (`id`), UNIQUE KEY (`id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

            try
            {
                BeginTransaction();

                // create the table
                ExecuteSQL(sSQL);


                insertSQL = insertSQL.Replace("(,`", "(`") + ") values (" + parametersSQL.Substring(1) + ")";

                // prepare the table to optimize for bulk insert
                ExecuteSQL("SET autocommit=0; SET unique_checks=0;");
                ExecuteSQL("LOCK TABLES " + schema + "`" + tableName + "` WRITE");

                // perform the inserts
                foreach (DataRow row in dt.Rows)
                {
                    ix = 0;
                    List<KeyValuePair<string, object>> paremeters = new List<KeyValuePair<string, object>>();
                    foreach (DataColumn column in dt.Columns)
                    {
                        paremeters.Add(new KeyValuePair<string, object>("@param" + ix, row[column]));
                        ix++;
                    }

                    ExecuteSQL(insertSQL, paremeters.ToArray());
                }

                // finish the bulk insert
                ExecuteSQL("UNLOCK TABLES");
                ExecuteSQL("SET unique_checks=1; SET autocommit=1; COMMIT;");

                Commit();

            }
            catch {
                RollBack();
                return false;
            }

            return true;
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyDefinition : System.Attribute
    {
        public bool IsPrimaryKey { get; set; }
        public bool AutoNumeric { get; set; }

        public PrimaryKeyDefinition(bool IsPrimaryKey, bool AutoNumeric = true)
        {
            this.IsPrimaryKey = IsPrimaryKey;
            this.AutoNumeric = AutoNumeric;
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RelatedField : System.Attribute
    {
        public RelatedField()
        {
        }
    }
}
