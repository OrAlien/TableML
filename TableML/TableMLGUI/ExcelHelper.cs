﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NPOI.SS.UserModel;
using TableML.Compiler;

namespace TableMLGUI
{
    public enum CheckType
    {
        /// <summary>
        /// 重复
        /// </summary>
        Repet,
        /// <summary>
        /// 空白
        /// </summary>
        Empty,
    }
    public class ExcelHelper
    {
        public static void UpdateAllTableSyntax()
        {
            var findPath = System.Environment.CurrentDirectory + ".\\..\\Src";
            //var findPath = @"e:\3dsn\plan\005ConfigTable\Src\";
            var files = Directory.GetFiles(findPath, "*.xls", SearchOption.AllDirectories);
            Console.WriteLine("开始更新{0}张Excel表", files.Length);
            var count = 0;
            foreach (string file in files)
            {
                var update = ToCSharpSyntax(file);
                if (update) count += 1;
            }
            Console.WriteLine("更新Excel表完成。");
            MessageBox.Show(string.Format("共整理{0}张表的前端字段为C#类型", count), "整理完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 替换成C#的数据类型
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>文件是否有改变</returns>
        public static bool ToCSharpSyntax(string filePath)
        {
            bool fileChange = false;
            IWorkbook Workbook;
            ISheet Worksheet;
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) // no isolation
            {
                try
                {
                    Workbook = WorkbookFactory.Create(file);
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("无法打开Excel: {0}, 可能原因：正在打开？或是Office2007格式（尝试另存为）？ {1}", filePath,
                        e.Message));
                    //IsLoadSuccess = false;
                }
            }
            Worksheet = Workbook.GetSheetAt(0);
            List<ICell> cells = Worksheet.GetRow(4).Cells;
            foreach (ICell cell in cells)
            {

                if (cell.StringCellValue == "num")
                {
                    cell.SetCellValue("int");
                    fileChange = true;
                }

                if (cell.StringCellValue == "str")
                {
                    cell.SetCellValue("string");
                    fileChange = true;
                }
                if (cell.StringCellValue.StartsWith("arr"))
                {
                    cell.SetCellValue("string");
                    fileChange = true;
                }
                if (cell.StringCellValue.StartsWith("ssg"))
                {
                    cell.SetCellValue("string");
                    fileChange = true;
                }
            }
            //文件没有改变，不保存
            if (!fileChange) return false;
            using (var memStream = new MemoryStream())
            {
                Workbook.Write(memStream);
                memStream.Flush();
                memStream.Position = 0;

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    var data = memStream.ToArray();
                    fileStream.Write(data, 0, data.Length);
                    fileStream.Flush();
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="type"></param>
        /// <param name="rowIdx">读取的行，从0开始</param>
        /// <returns></returns>
        public static bool CheckExcel(string filePath, CheckType type,int rowIdx = 5)
        {
            bool fileChange = false;
            IWorkbook Workbook;
            ISheet Worksheet;
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) // no isolation
            {
                try
                {
                    Workbook = WorkbookFactory.Create(file);
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("无法打开Excel: {0}, 可能原因：正在打开？或是Office2007格式（尝试另存为）？ {1}", filePath,
                        e.Message));
                    //IsLoadSuccess = false;
                }
            }

            Worksheet = Workbook.GetSheetAt(0);
            List<ICell> cells = Worksheet.GetRow(rowIdx).Cells;
            Dictionary<string, string> dict = new Dictionary<string, string>();
            StringBuilder repet = new StringBuilder();
            StringBuilder empty = new StringBuilder();
            foreach (ICell cell in cells)
            {
                if (dict.ContainsKey(cell.StringCellValue) == false)
                {
                    dict.Add(cell.StringCellValue, cell.StringCellValue);
                }
                else
                {
                    repet.AppendFormat("{0}\t", cell.StringCellValue);
                }
                if (string.IsNullOrEmpty(cell.StringCellValue))
                {
                    empty.AppendFormat("row:{0} column:{1} \t", cell.RowIndex + 1, cell.ColumnIndex + 1);
                }
            }
            switch (type)
            {
                case CheckType.Repet:
                    if (repet.Length >= 1)
                    {
                        Console.WriteLine("重复字段{0}", repet.ToString());
                        MessageBox.Show(repet.ToString(), "重复字段");
                    }
                    return repet.Length == 0;
                    break;
                case CheckType.Empty:
                    if (empty.Length >= 1)
                    {
                        Console.WriteLine("空白字段{0}", empty.ToString());
                        MessageBox.Show(empty.ToString(), "空白字段");
                    }
                    return empty.Length == 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

        }

        public static void CheckNameRepet(string[] filePaths)
        {
            int count = 0;
            foreach (string filePath in filePaths)
            {
                CheckExcel(filePath, CheckType.Repet);
            }
        }

        /// <summary>
        /// 检查前端字段名是否重复
        /// </summary>
        public static void CheckNameRepet(List<string> filePaths)
        {
            CheckNameRepet(filePaths.ToArray());
        }

        public static void CheckNameEmpty(string[] filePaths)
        {
            int count = 0;
            foreach (string filePath in filePaths)
            {
                CheckExcel(filePath, CheckType.Empty);
            }
        }
    }
}
