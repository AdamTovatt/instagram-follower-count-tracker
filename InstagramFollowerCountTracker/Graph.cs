using FollowerCountDatabaseTools;
using FollowerCountDatabaseTools.Models;
using ScottPlot;
using ScottPlot.Plottables;

namespace InstagramFollowerCountTracker
{
    public class Graph
    {
        private Dictionary<string, List<AccountInfoDataPoint>> dataSets;

        public Graph()
        {
            dataSets = new Dictionary<string, List<AccountInfoDataPoint>>();
        }

        public static async Task<Graph> CreateFromAccountNamesAsync(DatabaseManager database, params string[] accountNames)
        {
            Graph graph = new Graph();

            foreach (string accountName in accountNames)
            {
                List<AccountInfoDataPoint> dataPoints = await database.GetAccountInfoDataPointsAsync(accountName);

                if (dataPoints.Count == 0)
                    continue;

                graph.AddDataSet(accountName, dataPoints);
            }

            return graph;
        }

        public void AddDataSet(string name, List<AccountInfoDataPoint> dataPoints)
        {
            if (dataSets.ContainsKey(name))
            {
                dataSets[name] = dataPoints;
            }
            else
            {
                dataSets.Add(name, dataPoints);
            }
        }

        public byte[] Export(int width = 1000, int height = 800, bool darkMode = true)
        {
            DateTime? totalMinDate = null;
            int maxX = 0;

            foreach (string key in dataSets.Keys)
            {
                DateTime? minDate = dataSets[key].MinBy(x => x.RecordTime)?.RecordTime;

                if (minDate != null && (totalMinDate == null || minDate < totalMinDate))
                    totalMinDate = minDate;
            }

            if (totalMinDate == null)
                throw new Exception("Missing data points");

            Plot plot = new Plot();

            foreach (string key in dataSets.Keys)
            {
                double[] dataX = dataSets[key].Select(x => (x.RecordTime - totalMinDate).Value.TotalDays).ToArray();
                //double[] dataX = dataSets[key].Select(x => x.RecordTime.ToOADate()).ToArray();

                int[] dataY = dataSets[key].Select(x => x.Followers).ToArray();

                if (dataX.Last() > maxX)
                    maxX = (int)dataX.Last();

                Scatter scatter = plot.Add.Scatter(dataX, dataY);
                scatter.LineWidth = 4;
                scatter.MarkerStyle = MarkerStyle.None;

                Text text = plot.Add.Text(key, dataX.Last() + 0.5, dataY.Last());

                if (darkMode)
                    text.LabelFontColor = Color.FromHex("#d7d7d7");
            }

            plot.Title("Followers over Time");
            plot.XLabel("Days since " + totalMinDate.Value.ToString("yyyy-MM-dd"));
            plot.YLabel("Followers");
            plot.ShowLegend(); // Display a legend
            plot.Axes.SetLimitsX(0, maxX + 5);

            if (darkMode)
            {
                // change figure colors
                plot.FigureBackground.Color = Color.FromHex("#181818");
                plot.DataBackground.Color = Color.FromHex("#1f1f1f");

                // change axis and grid colors
                plot.Axes.Color(Color.FromHex("#d7d7d7"));
                plot.Grid.MajorLineColor = Color.FromHex("#404040");

                // change legend colors
                plot.Legend.BackgroundColor = Color.FromHex("#404040");
                plot.Legend.FontColor = Color.FromHex("#d7d7d7");
                plot.Legend.OutlineColor = Color.FromHex("#d7d7d7");
            }

            MemoryStream memoryStream = new MemoryStream();
            return plot.GetImageBytes(width, height, ImageFormat.Jpeg);
        }
    }
}
