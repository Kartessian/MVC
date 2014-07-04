using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCproject
{
    public class Database: IDisposable
    {
        public Database() { }

        public void Open() { }

        public void Close() { }

        public DataTable fillDataTable(string query)
        {
            DataTable result = new DataTable();

            // ... fill your datatable with your logic here ...

            result.Columns.Add("firstName");
            result.Columns.Add("lastName");

            result.Rows.Add("John", "Smith");
            result.Rows.Add("Paul", "Simons");

            return result;
        }

        public void Dispose() { }

    }
}
