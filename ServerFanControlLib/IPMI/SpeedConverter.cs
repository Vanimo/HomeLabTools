using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFanControlLib.IPMI
{
    public static class R710SpeedConverter
    {
        /* dec  hex     RPM         C   F
         * 00	00	    1200		
         * 01	01	    1200		
         * 02	02	    1320		
         * 03	03	    1560	    20	68
         * 04	04	    1680	    21	69,8
         * 05	05	    1800	    22	71,6
         * 06	06	    1920	    23	73,4
         * 07	07	    2040	    24	75,2
         * 08	08	    2160		
         * 09	09	    2280		
         * 10	0A	    2400		
         * 11	0B	    2400(-2520)		
         * 12	0C	    2520	    25	77
         * 13	0D	    2640		
         * 14	0E	    2760		
         * 15	0F	    2880		
         * 16	10	    3000		
         * 17	11	    3120	    26	78,8
         * 18	12	    3240		
         * 19	13	    3240(-3360)		
         * 20	14	    3360		
         * 21	15	    3480		
         * 22	16	    3600	    27	80,6    // Lowest/Default when running auto
         * 23	17	    3720		
         * 24	18	    3840		
         * 25	19	    3960		
         * 26	1A	    3960(-4200)		
         * 27	1B	    4200		
         * 28	1C	    4320		
         * 29	1D	    4440		
         * 30	1E	    4560		
         * 31	1F	    4560
         */

        public static Int32 FanSpeedFromIpmiSpeed(Int16 ipmiSpeed)
        {
            const Int32 multiplier = 120;
            // This is super dirty, but the speeds are not exactly linear compared to their dec/hex ipmi value.
            // Notice the jumps or freezes at 11, 19, 26

            if (ipmiSpeed < 11)
            {
                return (10 + ipmiSpeed) * multiplier;
            }

            if (ipmiSpeed < 19)
            {
                return (9 + ipmiSpeed) * multiplier;
            }

            if (ipmiSpeed < 26)
            {
                return (8 + ipmiSpeed) * multiplier;
            }

            return (7 + ipmiSpeed) * multiplier;
        }
    }
}
