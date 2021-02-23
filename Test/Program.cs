using System;
using General.Validate.CRC;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var Hexresult = new byte[] { 0xff, 0xa0 }.ToModbusCRC16(true);//General.Validate.CRC.CRC16.ToModbusCRC16(, true);
            var result2 = new byte[] { 0xff, 0xa0 }.ToCRC16(true);//General.Validate.CRC.CRC16.crc16(,2);
            var rec = new byte[] { 0xff, 0xa0 }.crc16();
            var rec1 = Convert.ToInt16(Hexresult,16);

            while (true){
                Console.WriteLine("Hello World!");
            }
        }
    }
}
