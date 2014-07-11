using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MVCproject
{
    public class FromCSV : IConverter
    {
        public DataTable ToDataTable(string source)
        {
            DataTable dt = new DataTable();

            string[] lines = source.Split('\n');

            // first row is supposed to be the headers
            // find the field delimiter: ',' or ';' - header should not have a , or ; in the name
            // so the char we find we use
            char delimiter = lines[0].Contains(";") ? ';' : ',';

            // want to evaluate each value in the CSV file to try to chose the right data type
            List<List<string>> content = new List<List<string>>();
            for (int i = 0; i < lines.Length; i++)
            {
                content.Add(getValues(lines[i], delimiter));
            }

            //remove empty lines
            content.RemoveAll(I => I.Count == 0);

            List<string> columns = content[0];
            for (int i = 0; i < columns.Count; i++)
            {
                // get the type for the column
                Type type = null;
                for (int line = 1; line < content.Count; line++)
                {
                    Type t = tryParse(content[line][i]);
                    if (type == null)
                    {
                        type = t;
                        continue;
                    }
                    if (t != type && type != null)
                    {
                        type = typeof(string);
                        break;
                    }
                }

                dt.Columns.Add(columns[i].ToValidName(), type);
            }

            // for each line add a row with the values
            for (var i = 1; i < content.Count; i++)
            {
                dt.Rows.Add(content[i].ToArray());
            }

            return dt;
        }

        private Type tryParse(string value)
        {
            double d = 0;
            DateTime t = DateTime.MinValue;
            if (double.TryParse(value, out d))
            {
                return typeof(double);
            }
            else
            {
                if (DateTime.TryParse(value, out t))
                {
                    return typeof(DateTime);
                }
            }
            return typeof(string);
        }

        private List<string> getValues(string line, char separador = ',')
        {
            List<string> values = new List<string>();

            StringBuilder value = new StringBuilder();
            bool insideQuote = false;
            if (line.EndsWith(separador.ToString())) line += " ";
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == separador)
                {
                    if (insideQuote)
                    {
                        value.Append(c);
                    }
                    else
                    {
                        if (value.Length == 0)
                        {
                            values.Add(null);
                        }
                        else
                        {
                            values.Add(value.ToString());
                        }
                        value.Clear();
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        insideQuote = !insideQuote;
                    }
                    else
                    {
                        value.Append(c);
                    }
                }
            }
            if (value.Length > 0)
            {
                values.Add(value.ToString());
            }

            return values;
        }
    }
}