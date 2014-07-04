using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MVCproject
{
    public class Database: IDisposable
    {
        private SqlConnection _conn;
        private SqlTransaction _tran;

        public Database(string connectionString) {

            _conn = new SqlConnection(connectionString);
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

        public void RollBack()
        {
            if (_tran != null)
            {
                _tran.Rollback();
                _tran.Dispose();
            }
        }



        private SqlCommand CreateCommand(string query, params KeyValuePair<string, object>[] Parameters)
        {
            SqlCommand comm;

            if (_tran != null)
                comm = new SqlCommand(query, _conn, _tran);
            else
                comm = new SqlCommand(query, _conn);

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

            return comm;
        }

        public int ExecuteSQL(string query, params KeyValuePair<string, object>[] Parameters)
        {
            SqlCommand comm = CreateCommand(query, Parameters);

            return comm.ExecuteNonQuery();
        }

        public object ExecuteScalar(string query, params KeyValuePair<string, object>[] Parameters)
        {
            SqlCommand comm = CreateCommand(query, Parameters);

            return comm.ExecuteScalar();
        }



        public DataTable GetDataTable(string query, params KeyValuePair<string, object>[] Parameters)
        {
            DataTable oDataTable = new DataTable();

            SqlCommand comm;
            if (_tran != null)
                comm = new SqlCommand(query, _conn, _tran);
            else
                comm = new SqlCommand(query, _conn);
            if (Parameters != null)
            {
                foreach (KeyValuePair<string, object> pair in Parameters)
                {
                    string skey = pair.Key;
                    if (!skey.StartsWith("@")) skey = "@" + skey;
                    if (pair.Value == null)
                    {
                        comm.Parameters.AddWithValue(skey, DBNull.Value);
                    }
                    else
                    {
                        comm.Parameters.AddWithValue(skey, pair.Value);
                    }
                }
            }
            SqlDataAdapter oDataAdapter = new SqlDataAdapter(comm);
            oDataAdapter.Fill(oDataTable);
            oDataAdapter.Dispose();
            comm.Dispose();

            return oDataTable;
        }



        public List<ITable> GetRecords<T>(KeyValuePair<string, object>[] parameters = null) where T : ITable, new()
        {
            List<ITable> result = new List<ITable>();

            T newObject = new T();

            List<string> Properties = GetObjectProperties<T>();
            DataTable dt = GetDataTable("select * from " + newObject.TableName, parameters);
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
