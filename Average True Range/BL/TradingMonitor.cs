using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using BL;
using DataLoading;

namespace BL
{
	public class TradingMonitor : IDisposable
	{
		public delegate void ReceiveCandleEvent(Candle candle, double? indicatorValue);
		public event ReceiveCandleEvent OnReceiveCandle;

		private IDataSource dataSource;
		private Thread monitorThread;
		private AverageTrueRange indicatorCalculator;

		private StockReportStream dataStream;
		private List<int> indicatorSerie;

		public TradingMonitor(IDataSource source, AverageTrueRange calculator)
		{
			dataSource = source;
			indicatorCalculator = calculator;
		}

		public void Dispose()
		{
			monitorThread.Abort();
		}

		public void RunMonitor()
		{
			dataStream = dataSource.GetReport();
			indicatorSerie = indicatorCalculator.Calculate(dataStream);

			monitorThread = new Thread(new ThreadStart(UpdateMonitor));
			monitorThread.Start();
		}

		protected void UpdateMonitor()
		{
		    int i = 0;
			while (dataStream.Candles.Count != 0)
			{
				OnReceiveCandle?.Invoke(dataStream.PopCandle(), indicatorSerie[i++]);
				Thread.Sleep(1000);
			}
		}

	}
}
