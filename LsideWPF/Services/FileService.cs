﻿namespace LsideWPF.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using CsvHelper;
    using LsideWPF.Utils;

    public class FileService
    {
        public IList<SlipLogEntry> GetSlipLogEntries(string path)
        {
            var dt = this.GetDataTable(path);
            var list = dt.ToList<SlipLogEntry>();
            return list;
        }

        public DataTable GetDataTable(string path)
        {
            DataTable dt;
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Do any configuration to `CsvReader` before creating CsvDataReader.
                using (var dr = new CsvDataReader(csv))
                {
                    dt = new DataTable();
                    dt.Load(dr);
                }
            }

            dt.DefaultView.Sort = "Time desc";
            dt = dt.DefaultView.ToTable();

            DataTable result = dt.Clone();

            for (int i = 2; i < result.Columns.Count; i++)
            {
                result.Columns[i].DataType = dt.Columns[i].DataType;
            }

            foreach (DataRow row in dt.Rows)
            {
                result.ImportRow(row);
            }

            return result;
        }

        public string GetBasePath()
        {
            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Properties.Settings.Default.LandingDirectory;
            string path = $"{myDocs}\\{dir}\\";
            return path;
        }

        /// <summary>
        /// GetList of FullFileNames.
        /// </summary>
        /// <param name="template">slipLog-*.csv .</param>
        /// <returns>List of filenames.</returns>
        private List<string> GetFullFiles(string template)
        {
            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Properties.Settings.Default.LandingDirectory;
            string path = $"{myDocs}\\{dir}\\";

            string[] files = Directory.GetFiles(path, template);
            List<string> result = new List<string>(files);
            return result;
        }

        /// <summary>
        /// GetList of (FileNames, fulfileNames).
        /// </summary>
        /// <param name="template">slipLog-*.csv .</param>
        private List<(string, string)> GetFiles(string template)
        {
            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Properties.Settings.Default.LandingDirectory;
            string path = $"{myDocs}\\{dir}\\";

            var files = Directory.EnumerateFiles(path, template);
            List<(string, string)> result = new List<(string, string)>();
            foreach (var file in files)
            {
                result.Add((Path.GetFileName(file), file));
            }

            return result;
        }
    }
}