using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace General.Bit
{
    public class BitUtil
    {
        public static int BitWiter(bool[] bools)
        {
            var ba = new BitArray(bools.Length);
            for (int i = 0; i < bools.Length; i++)
            {
                ba.Set(i, bools[i]);
            }
            return BitArray2Int(ba);
        }

        public static int BitWiter(int[] ints)
        {
            var ba = new BitArray(ints.Length);
            for (int i = 0; i < ints.Length; i++)
            {
                ba.Set(i, Convert.ToBoolean(ints[i]));
            }
            return BitArray2Int(ba);
        }

        /// <summary>
        /// 获取整数各位的值
        /// </summary>
        /// <param name="source">输入的源整数</param>
        /// <param name="index">位的下标</param>
        /// <param name="readType">eg: -1 读取所有，0 读取指定下标的位状态</param>
        /// <returns></returns>
        public static bool[] GetBitValue(int source, int index, int readType = -1)
        {
            var ba = new BitArray(new int[] { source });
            switch (readType)
            {
                case -1:
                    bool[] rec = new bool[ba.Length];
                    for (int j = 0; j < ba.Length; j++)
                    {
                        rec[j] = ba.Get(j);
                    }
                    return rec;
                case 0:
                    return new bool[] { ba.Get(index) };
                default:
                    throw new NotSupportedException(nameof(source));
            }
        }


        static byte ConvertToByte(BitArray bits)
        {
            if (bits.Count != 8)
            {
                throw new ArgumentException("bits");
            }
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);

            return bytes[0];
        }


        //
        // 摘要：
        //     将BitArray转换为一个十进制整数。
        // 参数：
        //     ba, 下表从低到高的顺序 与十进制整数的二进制形式从低到高的顺序一致
        static int BitArray2Int(BitArray ba)
        {
            Int32 ret = 0;
            for (Int32 i = 0; i < ba.Length; i++)
            {
                if (ba.Get(i))
                {
                    ret |= (1 << i);
                }
            }

            return ret;
        }
    }
}
