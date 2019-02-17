using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DataLoading;


namespace BL
{
    public class AverageTrueRange
    {
        public List<int>Calculate(StockReportStream stackReport)
        {
            List<int> Indicator  = new List<int>();
            List<Candle> listReport = stackReport.Candles.ToList();
            Indicator.Add(Convert.ToInt32(listReport[0].High - listReport[0].Low));
            for (int i = 1; i < listReport.Count; i++)
            {
                Indicator.Add(GetMaxValue(Convert.ToInt32(listReport[i].High - listReport[i].Low), Convert.ToInt32(listReport[i].High - listReport[i - 1].Close),
                    Convert.ToInt32(listReport[i - 1].Close - listReport[i].Low)));
            }
            return Indicator;
        }

        private int GetMaxValue(int n1, int n2, int n3)
        {
            return Math.Max(Math.Max(n1, n2), n3);
        }
    }
}
