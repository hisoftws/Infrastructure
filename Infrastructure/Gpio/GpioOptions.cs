using Infrastructure.Base;

namespace Infrastructure.Gpio
{
    public class GpioOptions : LedBase
    {
        public GpioOptions()
        {

        }

        /// <summary>
        /// 针脚使用方式
        /// eg：
        /// 0 is Logical model
        /// 1 is Board model
        /// </summary>
        public int scheme { get; set; }

    }
}
