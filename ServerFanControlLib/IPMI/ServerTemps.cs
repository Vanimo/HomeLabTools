using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFanControlLib
{
    public class ServerTemps
    {
        private Single? _ambient;
        private Single? _mainboard;

        public String PrintAmbient => (_ambient?.ToString(CultureInfo.InvariantCulture) ?? "N/A") + " degC";

        public String PrintBoard => (_mainboard?.ToString(CultureInfo.InvariantCulture) ?? "N/A" ) + " degC" ;

        public void SetAmbient(string ambient)
        {
            _ambient = ParseTemperature(ambient);
        }

        public void SetMainboard(string mainboard)
        {
            _mainboard = ParseTemperature(mainboard);
        }

        public Single Ambient => _ambient ?? -274f;

        public static Single? ParseTemperature(string sTemp)
        {
            sTemp = sTemp.Trim().ToUpperInvariant();
            if (!sTemp.EndsWith("C"))
            {
                return ParseFromFahrenheit(sTemp);
            }

            sTemp = sTemp.Split(' ')[0];
            sTemp = sTemp.TrimEnd('C');

            if (Single.TryParse(
                sTemp,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var temp))
            {
                return temp;
            }

            return null;
        }

        public static float? ParseFromFahrenheit(string sTemp)
        {
            if (!sTemp.EndsWith("F"))
            {
                return null;
            }

            sTemp = sTemp.Split(' ')[0];
            sTemp = sTemp.TrimEnd('F');

            if (Single.TryParse(
                sTemp,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var temp))
            {
                return (temp - 32) * 5 / 9;
            }

            return null;
        }

        public Boolean IsValid()
        {
            return _ambient.HasValue;
        }
    }
}
