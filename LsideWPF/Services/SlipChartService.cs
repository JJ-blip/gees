namespace LsideWPF.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CsvHelper;
    using LsideWPF.Services;

    public class SlipChartService
    {
        public SlipChartService() { }

        public DataTable GetLandingLogEntries()
        {
            string path = $"c:\\Users\\JAF-I\\Documents\\slip.csv";
            var dt = new DataTable();
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Do any configuration to `CsvReader` before creating CsvDataReader.
                using (var dr = new CsvDataReader(csv))
                {
                    dt.Load(dr);
                }
            }

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
                    Fpm = int.Parse((string)dt.Rows[row][1]),
                    Altitude = int.Parse((string)dt.Rows[row][2]),
                    AirSpeedInd = Convert.ToDouble((string)dt.Rows[row][3]),
                    GroundSpeed = Convert.ToDouble((string)dt.Rows[row][4]),
                    HeadWind = Convert.ToDouble((string)dt.Rows[row][5]),
                    CrossWind = Convert.ToDouble((string)dt.Rows[row][6]),
                    SlipAngle = Convert.ToDouble((string)dt.Rows[row][7]),
                    ForwardSlipAngle = Convert.ToDouble((string)dt.Rows[row][8]),
                    SideSlipAngle = Convert.ToDouble((string)dt.Rows[row][9]),
                    BankAngle = Convert.ToDouble((string)dt.Rows[row][10]),
                    DriftAngle = Convert.ToDouble((string)dt.Rows[row][11]),
                    CrashFlag = (string)dt.Rows[row][12],
                };
                result.Add(slipLogEntry);
            }

            return result;
        }
    }
}
