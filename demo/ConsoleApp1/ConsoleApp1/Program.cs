﻿using Microsoft.CodeAnalysis.CSharp.Scripting;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var m = match("TOTAL", "TOTAL");
            var str = "INVOICE DATE(發票日期):20180904".Split(':')[1];
            string result = await CSharpScript.EvaluateAsync<string>("\"INVOICE DATE(發票日期):20180904\".Split(':')[1]");
            var configpath = @"d:\XslImportRule1.xml";
            var xdoc = XDocument.Load(configpath);
            var root = xdoc.Root.Name;
            var descxml = new XDocument();
            descxml.Add(new XElement(xdoc.Root.Name));


            var path = @"d:\9C箱单0000880680.xlsx";
            var workbook = new XSSFWorkbook(path);

            foreach (var element in xdoc.Root.Elements())
            {
                var name = element.Name;
                var atts = element.Attributes();
                var replicate = atts.Where(x => x.Name == "replicate").FirstOrDefault()?.Value;
                var sheetname = atts.Where(x => x.Name == "sheet-name").FirstOrDefault()?.Value;
                var sheetnum = atts.Where(x => x.Name == "sheet-num").FirstOrDefault()?.Value;
                var starttag = atts.Where(x => x.Name == "start-tag").FirstOrDefault()?.Value;
                var start = atts.Where(x => x.Name == "start").FirstOrDefault()?.Value;
                var endtag = atts.Where(x => x.Name == "end-tag").FirstOrDefault()?.Value;
                var end = atts.Where(x => x.Name == "end").FirstOrDefault()?.Value;
                ISheet sheet;
                if (!string.IsNullOrEmpty(sheetname))
                {
                    sheet = workbook.GetSheet(sheetname);
                }
                else if (!string.IsNullOrEmpty(sheetnum))
                {
                    sheet = workbook.GetSheetAt(Convert.ToInt32(sheetnum));
                }
                else
                {
                    sheet = workbook.GetSheetAt(0);
                }
                CellAddress startaddress;
                CellAddress endaddress;
                if (replicate == "true")
                {
                    var table = new DataTable();
                    #region test
                    
                    if (!string.IsNullOrEmpty(starttag))
                    {
                        startaddress = findXslx(sheet, starttag);
                    }
                    else if (!string.IsNullOrEmpty(start))
                    {
                        startaddress = new CellAddress(new CellReference(start));
                    }
                    else
                    {
                        startaddress = new CellAddress(new CellReference("A0"));
                    }
                    if (!string.IsNullOrEmpty(endtag))
                    {
                        endaddress = findXslx(sheet, endtag);
                    }
                    else if (!string.IsNullOrEmpty(end))
                    {
                        endaddress = new CellAddress(new CellReference(end));
                    }
                    else
                    {
                        endaddress = null;
                    }
                    #endregion
                    var firstrow = startaddress == null ? sheet.FirstRowNum : startaddress.Row;
                    var lastrow = (endaddress == null) ? sheet.LastRowNum : endaddress.Row;

                    for (int r = firstrow; r < lastrow; r++)
                    {
                        var row = sheet.GetRow(r);
                        if (row == null) continue;
                        var lastcell = row.LastCellNum;
                        var firstcell = row.FirstCellNum;
                        if (r == firstrow)
                        {
                            for (int c = firstcell; c < lastcell; c++)
                            {
                                var cell = row.GetCell(c);
                                if (cell == null) continue;
                                var strval = getCellValue(cell).Trim();
                                if (!string.IsNullOrEmpty(strval))
                                {
                                    table.Columns.Add(new DataColumn(strval));
                                }
                            }
                        }
                        else
                        {
                            var dataRow = table.NewRow();
                            var array = new string[table.Columns.Count];
                            for (var c = 0; c < table.Columns.Count; c++)
                            {
                                var cell = row.GetCell(c);
                                var val = getCellValue(cell).Trim();
                                array[c] = val;
                            }
                            dataRow.ItemArray = array;
                            table.Rows.Add(dataRow);
                        }
                    }

                    if (table.Rows.Count > 0)
                    {
                        var mapfields = element.Elements();
                        foreach (DataRow dr in table.Rows)
                        {
                            var xelement = new XElement(name);
                            foreach (var ele in mapfields)
                            {
                                var elename = ele.Name;
                                var fieldname = ele.Attributes().Where(x => x.Name == "data-field").Select(x => x.Value).FirstOrDefault();
                                var datatype = ele.Attributes().Where(x => x.Name == "data-type").Select(x => x.Value);
                                var fieldval = table.Columns.Contains(fieldname) ? dr[fieldname] : "";
                                var xfelement = new XElement(elename, fieldval);
                                xelement.Add(xfelement);
                            }
                            descxml.Root.Add(xelement);

                        }

                    }
                }
                else
                {
                    var xhead = new XElement(name);
                    foreach(var subele in element.Elements()) {
                        
                        var subname = subele.Name;
                        var subelement = new XElement(subname);
                        var subatts = subele.Attributes();
                        var substarttag = subatts.Where(x => x.Name == "start-tag").FirstOrDefault()?.Value;
                        var substart = subatts.Where(x => x.Name == "start").FirstOrDefault()?.Value;
                        var formatter = subatts.Where(x => x.Name == "data-formatter").FirstOrDefault()?.Value;
                        var offset = subatts.Where(x => x.Name == "data-offset").FirstOrDefault()?.Value;
                        var defaultvalue = subatts.Where(x => x.Name == "data-default").FirstOrDefault()?.Value;
                        CellAddress celladdress=null;
                        if (!string.IsNullOrEmpty(substarttag))
                        {
                            celladdress = findXslx(sheet, substarttag);
                        }
                        else if (!string.IsNullOrEmpty(substart)) {
                            celladdress = new CellAddress(new CellReference(substart));
                        }
                        if (celladdress != null)
                        {
                            var r = 0;
                            var c = 0;
                            if (!string.IsNullOrEmpty(offset)) {
                                var sp = offset.Split(';');
                                foreach (var ts in sp) {
                                    var sparray = ts.Split(':');
                                    if(sparray[0].Equals("c", StringComparison.OrdinalIgnoreCase))
                                    {
                                        c = Convert.ToInt32(sparray[1]);
                                    }
                                    else
                                    {
                                        r = Convert.ToInt32(sparray[1]);
                                    }
                                }
                            }
                            var cell = sheet.GetRow(celladdress.Row + r).GetCell(celladdress.Column+c);
                            var val = getCellValue(cell);
                            if (string.IsNullOrEmpty(val) && !string.IsNullOrEmpty(defaultvalue))
                            {
                                val = defaultvalue;
                            }
                            if (!string.IsNullOrEmpty(formatter))
                            {
                                var codescript = formatter.Replace("$", "\"" + val + "\"");
                                var fval = await CSharpScript.EvaluateAsync<string>(codescript);
                                val = fval;
                            }
                            subelement.SetValue(val);
                        }
                        else if (!string.IsNullOrEmpty(defaultvalue)) {
                            subelement.SetValue(defaultvalue);
                        }
                        xhead.Add(subelement);

                    }
                    descxml.Root.Add(xhead);
                }
            }
            descxml.Save("d:\\output.xml");
            //var sheet = workbook.GetSheetAt(0);

        }

        static CellAddress findXslx(ISheet sheet, string key)
        {
            var lastrow = sheet.LastRowNum;
            var firstrow = sheet.FirstRowNum;
            for (int r = firstrow; r < lastrow; r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;
                var lastcell = row.LastCellNum;
                var firstcell = row.FirstCellNum;
                for (int c = firstcell; c < lastcell; c++)
                {
                    var cell = row.GetCell(c);
                    if (cell == null) continue;
                    var strval = getCellValue(cell).Trim();
                    //if (strval.Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    return cell.Address;
                    //}
                    if (match( key, strval)) {
                        return cell.Address;
                    }
                }
            }
            return null;
        }
        private static string getCellValue(ICell cell)
        {
            if (cell == null)
            {
                return string.Empty;
            }
            var dataFormatter = new DataFormatter(CultureInfo.CurrentCulture);

            // If this is not part of a merge cell,
            // just get this cell's value like normal.
            if (!cell.IsMergedCell)
            {
                return dataFormatter.FormatCellValue(cell);
            }

            // Otherwise, we need to find the value of this merged cell.
            else
            {
                // Get current sheet.
                var currentSheet = cell.Sheet;

                // Loop through all merge regions in this sheet.
                for (int i = 0; i < currentSheet.NumMergedRegions; i++)
                {
                    var mergeRegion = currentSheet.GetMergedRegion(i);

                    // If this merged region contains this cell.
                    if (mergeRegion.FirstRow <= cell.RowIndex && cell.RowIndex <= mergeRegion.LastRow &&
                        mergeRegion.FirstColumn <= cell.ColumnIndex && cell.ColumnIndex <= mergeRegion.LastColumn)
                    {
                        // Find the top-most and left-most cell in this region.
                        var firstRegionCell = currentSheet.GetRow(mergeRegion.FirstRow)
                                                .GetCell(mergeRegion.FirstColumn);

                        // And return its value.
                        return dataFormatter.FormatCellValue(firstRegionCell);
                    }
                }
                // This should never happen.
                throw new Exception("Cannot find this cell in any merged region");
            }
        }

        static bool match(string pattern, string input)
        {
            if (String.Compare(pattern, input) == 0)
            {
                return true;
            }
            else if (String.IsNullOrEmpty(input))
            {
                if (String.IsNullOrEmpty(pattern.Trim(new Char[1] { '*' })))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (pattern.Length == 0)
            {
                return false;
            }
            else if (pattern[0] == '?')
            {
                return match(pattern.Substring(1), input.Substring(1));
            }
            else if (pattern[pattern.Length - 1] == '?')
            {
                return match(pattern.Substring(0, pattern.Length - 1),
                                           input.Substring(0, input.Length - 1));
            }
            else if (pattern[0] == '*')
            {
                if (match(pattern.Substring(1), input))
                {
                    return true;
                }
                else
                {
                    return match(pattern, input.Substring(1));
                }
            }
            else if (pattern[pattern.Length - 1] == '*')
            {
                if (match(pattern.Substring(0, pattern.Length - 1), input))
                {
                    return true;
                }
                else
                {
                    return match(pattern, input.Substring(0, input.Length - 1));
                }
            }
            else if (pattern[0] == input[0])
            {
                return match(pattern.Substring(1), input.Substring(1));
            }
            return false;
        }
    }
}