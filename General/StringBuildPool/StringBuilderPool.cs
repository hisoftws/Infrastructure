using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace General.StringBuildPool
{
    // StringBuilder 的建立也会耗费大量的资源，因此共用他们，使用这个类来管理池子
    // 需要了从这里获取一个，用完自动释放 ItemHolder 即可释放会池子
    // 例如  using (var itemHolder = pool.Acquire()){}
    public class StringBuilderPool
    {
        private StringBuilder _fastPool;
        private readonly StringBuilder[] _slowPool;
        private readonly int _maxBuilderCapacity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="poolCapacity">Max number of items</param>
        /// <param name="initialBuilderCapacity">Initial StringBuilder Size</param>
        /// <param name="maxBuilderCapacity">Max StringBuilder Size</param>
        public StringBuilderPool(int poolCapacity, int initialBuilderCapacity = 1024, int maxBuilderCapacity = 512 * 1024)
        {
            //一个快速池子，一直保持
            _fastPool = new StringBuilder(10 * initialBuilderCapacity);
            // 一个慢速池子，如果快速池子被占用，则从慢速池子取一个
            _slowPool = new StringBuilder[poolCapacity];
            for (int i = 0; i < _slowPool.Length; ++i)
            {
                _slowPool[i] = new StringBuilder(initialBuilderCapacity);
            }
            _maxBuilderCapacity = maxBuilderCapacity;
        }

        /// <summary>
        /// Takes StringBuilder from pool
        /// </summary>
        /// <returns>Allow return to pool</returns>
        public ItemHolder Acquire()
        {
            StringBuilder item = _fastPool;

            if (item == null || item != Interlocked.CompareExchange(ref _fastPool, null, item))
            {
                for (int i = 0; i < _slowPool.Length; i++)
                {
                    item = _slowPool[i];
                    if (item != null && item == Interlocked.CompareExchange(ref _slowPool[i], null, item))
                    {
                        return new ItemHolder(item, this, i);
                    }
                }
                return new ItemHolder(new StringBuilder(), null, 0);
            }
            else
            {
                return new ItemHolder(item, this, -1);
            }
        }

        /// <summary>
        /// Releases StringBuilder back to pool at its right place
        /// </summary>
        private void Release(StringBuilder stringBuilder, int poolIndex)
        {
            if (stringBuilder.Length > _maxBuilderCapacity)
            {
                // Avoid high memory usage by not keeping huge StringBuilders alive (Except one StringBuilder)
                int maxBuilderCapacity = poolIndex == -1 ? _maxBuilderCapacity * 10 : _maxBuilderCapacity;
                if (stringBuilder.Length > maxBuilderCapacity)
                {
                    stringBuilder = new StringBuilder(maxBuilderCapacity / 2);
                }
            }

            stringBuilder.Length = 0;

            if (poolIndex == -1)
                _fastPool = stringBuilder;
            else
                _slowPool[poolIndex] = stringBuilder;
        }

        /// <summary>
        /// Keeps track of acquired pool item
        /// </summary>
        public struct ItemHolder : IDisposable
        {
            public readonly StringBuilder Item;
            readonly StringBuilderPool _owner;
            readonly int _poolIndex;

            public ItemHolder(StringBuilder stringBuilder, StringBuilderPool owner, int poolIndex)
            {
                Item = stringBuilder;
                _owner = owner;
                _poolIndex = poolIndex;
            }

            /// <summary>
            /// Releases pool item back into pool
            /// </summary>
            public void Dispose()
            {
                if (_owner != null)
                {
                    _owner.Release(Item, _poolIndex);
                }
            }
        }
    }
}
