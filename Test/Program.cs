using System;
using General.Validate.CRC;
using General.Bit;

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

            int[] ints = new int[] { 1, 1, 1, 0, 0, 0, 0, 0 };
            var result = BitUtil.BitWiter(ints);

            bool[] bools = new bool[] { true, true, true, false, false, false, false, false };
            result = BitUtil.BitWiter(bools);


            bools = BitUtil.GetBitValue(result, 0, readType: -1);

            while (true){
                Console.WriteLine("Hello World!");
            }
        }
    }
}
