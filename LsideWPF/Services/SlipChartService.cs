namespace LsideWPF.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using CsvHelper;
    using CsvHelper.Configuration;

    public class SlipChartService
    {
        public SlipChartService()
        {
        }

        public DataTable GetLandingLogEntries(string path)
        {
            var dt = new DataTable();

            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ReadingExceptionOccurred = this.IgnoreReadingException,
            };

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, config))
            {
                try
                {
                    using (var dr = new CsvDataReader(csv))
                    {
                        dt.Load(dr);
                    }
                 }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"While reading Slip Log file {path}", ex);
                }
            }

            if (dt.Rows.Count > 0)
            {
                dt.DefaultView.Sort = "Time desc";
                dt = dt.DefaultView.ToTable();

                DataTable clone = dt.Clone();

                for (int i = 2; i < clone.Columns.Count; i++)
                {
                    clone.Columns[i].DataType = dt.Columns[i].DataType;
                }

                foreach (DataRow row in dt.Rows)
                {
                    clone.ImportRow(row);
                }
            }

            return dt;
        }

        public List<SlipLogEntry> GetSlipLogEntries(DataTable dt)
        {
            var result = new List<SlipLogEntry>();

            for (int row = 0; row < dt.Rows.Count; row++)
            {
                SlipLogEntry slipLogEntry = new SlipLogEntry
                {
                    Time = DateTime.Parse((string)dt.Rows[row][0]),
                    Altitude = int.Parse((string)dt.Rows[row][1]),
                    VerticalSpeed = int.Parse((string)dt.Rows[row][2]),
                    AirSpeedInd = Convert.ToDouble((string)dt.Rows[row][3]),
                    GroundSpeed = Convert.ToDouble((string)dt.Rows[row][4]),
                    RelativeWindZ = Convert.ToDouble((string)dt.Rows[row][5]),
                    RelativeWindX = Convert.ToDouble((string)dt.Rows[row][6]),
                    SlipAngle = Convert.ToDouble((string)dt.Rows[row][7]),
                    BankAngle = Convert.ToDouble((string)dt.Rows[row][8]),
                    DriftAngle = Convert.ToDouble((string)dt.Rows[row][9]),
                    Heading = int.Parse((string)dt.Rows[row][10]),
                    Headwind = Convert.ToDouble((string)dt.Rows[row][11]),
                    Crosswind = Convert.ToDouble((string)dt.Rows[row][12]),
                };
                result.Add(slipLogEntry);
            }

            return result;
        }

        private bool IgnoreReadingException(ReadingExceptionOccurredArgs args)
        {
            // discard the error row;
            return false;
        }
    }
}
