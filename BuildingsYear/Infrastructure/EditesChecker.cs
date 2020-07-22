using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildingsYear.Infrastructure
{
    public static class EditesChecker
    {
        static int _currCntPerDay;
        static DateTime _currDay;

        public static bool IsMaxEditesPerDay()
        {
            DateTime now = DateTime.Now;
            if (now.Date == _currDay.Date && _currCntPerDay > 200) 
            {
                return false;
            }
            return true;
        }

        public static void AddEditsCnt()
        {
            DateTime now = DateTime.Now;
            if (now.Date == _currDay.Date)
            {
                _currCntPerDay += 1;
            }
            else
            {
                _currDay = now;
                _currCntPerDay += 1;
            }
        }
    }
}
