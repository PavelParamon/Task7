using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SciChart;
using SciChart.Charting.Model.DataSeries;
using DataLoading;
using BL;
using SciChart.Data.Model;
using System.Windows.Threading;
using Microsoft.Win32;

namespace TradeAnalyzer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private OhlcDataSeries<DateTime, double> ohlcSeries;
		private XyDataSeries<DateTime, double> sarSeries;

		private TradingMonitor tradingMonitor;

		private int candlesCount = 0;

		public MainWindow()
		{
			InitializeComponent();

			ReloadChartSeries();
			StockChart.XAxis.AutoRange = SciChart.Charting.Visuals.Axes.AutoRange.Once;

			LoadDataSource(new XLSXDataLoader(@"C:\Users\Pavel\Desktop\Task_7-master\Average True Range\Source\prices.xlsx"));
			this.Loaded += OnLoaded;
		}

		private void ReloadChartSeries()
		{
			ohlcSeries = new OhlcDataSeries<DateTime, double>() { SeriesName = "Candles", FifoCapacity = 10000 };
			sarSeries = new XyDataSeries<DateTime, double>() { SeriesName = "Sar", FifoCapacity = 10000 };

			CandleSeries.DataSeries = ohlcSeries;
			SarSeries.DataSeries = sarSeries;
		}

		private void AddNewCandle(Candle candle, double? indicatorValue)
		{
			using (ohlcSeries.SuspendUpdates())
			using (sarSeries.SuspendUpdates())
			{
				ohlcSeries.Append(candle.Time, (double)candle.Open, (double)candle.High, (double)candle.Low, (double)candle.Close);

				if (indicatorValue != null)
					sarSeries.Append(candle.Time, indicatorValue.Value);

				candlesCount++;

				StockChart.XAxis.VisibleRange = new IndexRange(candlesCount - 50, candlesCount);
			}
		}

		private void OnCandleReceived(Candle candle, double? indicatorValue)
		{
			StockChart.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { AddNewCandle(candle, indicatorValue); }));
		}

		private void MainMenu_File_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "XLSX files (*.xlsx)|*.xlsx|JSON files (*.json)|*.json";

			if (openFileDialog.ShowDialog() == true)
			{
				try
				{
					string extension = System.IO.Path.GetExtension(openFileDialog.FileName);

					if (extension == ".xlsx")
						LoadDataSource(new XLSXDataLoader(openFileDialog.FileName));
					else if (extension == ".json")
						LoadDataSource(new JSONDataLoader(openFileDialog.FileName));
					else
						throw new Exception();
				}
				catch (Exception error)
				{
					MessageBox.Show("Произошла ошибка загрузки данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void LoadDataSource(IDataSource source)
		{
			if (this.tradingMonitor != null) {
				tradingMonitor.Dispose();
				tradingMonitor = null;
			}

			tradingMonitor = new TradingMonitor(source, new AverageTrueRange());
			tradingMonitor.OnReceiveCandle += OnCandleReceived;

			ReloadChartSeries();

			tradingMonitor.RunMonitor();
		}

		private void MainMenu_Exit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
		}

	}
}
