using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using OfficeOpenXml;

namespace MVCproject
{
    public class FromExcel : IConverter
    {
        /// <summary>
        /// In this method the source should be the path to the file
        /// instead the content of it
        /// </summary>
        public DataTable ToDataTable(string source)
        {
            DataTable dt = new DataTable();

            // Very Basic Excel import, will read the first sheet looking to start on cell 1,1

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(source);

            using (ExcelPackage excel = new ExcelPackage(fileInfo))
            {
                ExcelWorkbook book = excel.Workbook;
                if (book != null && book.Worksheets.Count > 0)
                {
                    ExcelWorksheet sheet = book.Worksheets[1];

                    int iPos = 1;
                    while (sheet.Cells[1, iPos].Value != null)
                    {
                        dt.Columns.Add(sheet.Cells[1, iPos].Value.ToString().ToValidName(), sheet.Cells[2, iPos].Value.GetType());
                        
                        iPos++;
                    }

                    int iRow = 2;
                    while (true)
                    {
                        bool isEmpty = true;
                        DataRow row = dt.NewRow();

                        for (int i = 1; i < iPos; i++)
                        {
                            var value = sheet.Cells[iRow, i].Value;

                            if (value != null)
                            {
                                isEmpty = false;
                                row[i - 1] = value;
                            }
                            else
                            {
                                row[i - 1] = DBNull.Value;
                            }
                        }

                        if (isEmpty) break;

                        dt.Rows.Add(row);
                        iRow++;
                    }

                }

            }

            return dt;
        }

    }
}