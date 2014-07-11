using System;
using System.Data;
using System.Linq;

namespace MVCproject
{
    public interface IConverter
    {
        DataTable ToDataTable(string source);
    }
}
