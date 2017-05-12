using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TableML.Compiler
{
    /// <summary>
    /// TSV��ʽ��֧��
    /// </summary>
    public class SimpleTSVFile : ITableSourceFile
    {
        public Dictionary<string, int> ColName2Index { get; set; }
        public Dictionary<int, string> Index2ColName { get; set; }
        public Dictionary<string, string> ColName2Statement { get; set; }
        public Dictionary<string, string> ColName2Comment { get; set; }
        public string ExcelFileName { get; set; }

        private TableFile _tableFile;
        private int _columnCount;
        public SimpleTSVFile(string filePath)
        {
            ColName2Index = new Dictionary<string, int>();
            Index2ColName = new Dictionary<int, string>();
            ColName2Statement = new Dictionary<string, string>();
            ColName2Comment = new Dictionary<string, string>();
            ExcelFileName = Path.GetFileName(filePath);
            try
            {
                ParseTsv(filePath);
            }
            catch (Exception e)
            {
                throw new Exception("Error TSV File: " + filePath + " / " + e.Message);
            }
        }

        private void ParseTsv(string filePath)
        {
            //NOTE ������޷��򿪣��������ļ������ʽ�����⣡
            _tableFile = TableFile.LoadFromFile(filePath);
            _columnCount = _tableFile.GetColumnCount();


            // ͨ��TableFileע��ͷ��Ϣ
            var commentRow = _tableFile.GetRow(1);
            foreach (var kv in _tableFile.Headers)
            {
                var header = kv.Value;
                ColName2Index[header.HeaderName] = header.ColumnIndex;
                Index2ColName[header.ColumnIndex] = header.HeaderName;
                ColName2Statement[header.HeaderName] = header.HeaderMeta;
                ColName2Comment[header.HeaderName] = commentRow[header.ColumnIndex];
            }
        }
        public int GetRowsCount()
        {
            return _tableFile.GetRowCount() - 1; // ��ȥע����
        }

        public int GetColumnCount()
        {
            return _columnCount;
        }

        public string GetString(string columnName, int dataRow)
        {
            return _tableFile.GetRow(dataRow + 1 + 1)[columnName]; // 1�п�ʼ�����Ҷ���˵���У�+2
        }
    }
}