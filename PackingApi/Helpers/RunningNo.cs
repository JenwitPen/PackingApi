using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Helpers
{
    static public class RunningNo
    {
        static public String GetRunNoYYYYMMdd(int runNo)
        {
            String strZero = "";
            if (runNo < 999)
            {
                strZero = "0";
            }
            if (runNo < 99)
            {
                strZero += "0";
            }
            if (runNo < 9)
            {
                strZero += "0";
            }
            string strNum = DateTime.Now.Year.ToString() + (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString()) + strZero + runNo;
            return strNum;
        }
    }
}
