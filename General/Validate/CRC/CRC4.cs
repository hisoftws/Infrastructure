using System;
using System.Collections.Generic;
using System.Text;

namespace General.Validate.CRC
{
    public static class CRC4
    {
        public static byte ToCRC4(this byte[] data, int length)
        {
            byte i;
            byte crc = 0;
            int index = 0;
            while (length-- > 0)
            {
                crc ^= data[index];
                index++;
                for (i = 0; i < 8; ++i)
                {
                    if ((crc & 1) == 1)
                        crc = (byte)((crc >> 1) ^ 0x0C);
                    else
                        crc = (byte)(crc >> 1);
                }
            }
            return crc;
        }
    }
}
