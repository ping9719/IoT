using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// 固定容量的字节队列（先进先出）
    /// </summary>
    public sealed class QueueByteFixed
    {
        private readonly byte[] _buffer;
        private int _head;
        private int _tail;
        private int _count;
        private bool _allowOverwrite;
        private readonly object _syncRoot = new object();

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity { get; }
        /// <summary>
        /// 是否允许覆盖
        /// </summary>
        public bool IsOverwrite
        {
            get { lock (_syncRoot) { return _allowOverwrite; } }
            set { lock (_syncRoot) { _allowOverwrite = value; } }
        }
        /// <summary>
        /// 队列中的元素数量
        /// </summary>
        public int Count
        {
            get { lock (_syncRoot) { return _count; } }
        }
        /// <summary>
        /// 队列是否为空
        /// </summary>
        public bool IsEmpty
        {
            get { lock (_syncRoot) { return _count == 0; } }
        }
        /// <summary>
        /// 队列是否已满
        /// </summary>
        public bool IsFull
        {
            get { lock (_syncRoot) { return _count == Capacity; } }
        }
        /// <summary>
        /// 队列
        /// </summary>
        /// <param name="capacity">容量</param>
        /// <param name="allowOverwrite">是否允许循环覆盖</param>
        /// <exception cref="ArgumentException"></exception>
        public QueueByteFixed(int capacity, bool allowOverwrite)
        {
            if (capacity <= 0)
                throw new ArgumentException("容量必须大于0");

            Capacity = capacity;
            _buffer = new byte[Capacity];
            _allowOverwrite = allowOverwrite;
            _head = _tail = _count = 0;
        }
        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="data">数据</param>
        /// <exception cref="InvalidOperationException">队列已满</exception>
        public void Enqueue(byte data)
        {
            lock (_syncRoot)
            {
                if (_count == Capacity)
                {
                    if (!_allowOverwrite)
                        throw new InvalidOperationException("队列已满。");

                    _head = (_head + 1) % Capacity;
                    _count--;
                }

                _buffer[_tail] = data;
                _tail = (_tail + 1) % Capacity;
                _count = Math.Min(_count + 1, Capacity);
            }
        }
        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="size">大小</param>
        /// <exception cref="ArgumentOutOfRangeException">偏移量不在范围</exception>
        /// <exception cref="ArgumentException">无效的大小或偏移量</exception>
        /// <exception cref="InvalidOperationException">超过容量或者容量不足</exception>
        public void Enqueue(byte[] data, int offset = 0, int size = 0)
        {
            if (data == null || data.Length == 0)
                return;

            lock (_syncRoot)
            {
                // 计算实际要复制的数据长度
                int actualSize = (size <= 0) ? data.Length - offset : size;
                if (offset < 0 || offset >= data.Length)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (actualSize <= 0 || offset + actualSize > data.Length)
                    throw new ArgumentException("无效的大小或偏移量。");

                if (actualSize > Capacity)
                {
                    if (!_allowOverwrite)
                        throw new InvalidOperationException("数据超过队列容量.");

                    // 只保留最后 Capacity 个字节
                    int startIndex = offset + actualSize - Capacity;
                    Array.Copy(data, startIndex, _buffer, 0, Capacity);
                    _head = 0;
                    _tail = 0;
                    _count = Capacity;
                    return;
                }

                // 计算需要覆盖的旧数据量
                int overflow = actualSize - (Capacity - _count);
                if (overflow > 0)
                {
                    if (!_allowOverwrite)
                        throw new InvalidOperationException("容量不足.");

                    _head = (_head + overflow) % Capacity;
                    _count -= overflow;
                }

                // 分两种情况复制数据到缓冲区
                int spaceUntilEnd = Capacity - _tail;
                if (actualSize <= spaceUntilEnd)
                {
                    Array.Copy(data, offset, _buffer, _tail, actualSize);
                }
                else
                {
                    int firstPart = spaceUntilEnd;
                    int secondPart = actualSize - firstPart;
                    Array.Copy(data, offset, _buffer, _tail, firstPart);
                    Array.Copy(data, offset + firstPart, _buffer, 0, secondPart);
                }

                _tail = (_tail + actualSize) % Capacity;
                _count = Math.Min(_count + actualSize, Capacity);
            }
        }
        /// <summary>
        /// 出队
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">队列为空</exception>
        public byte Dequeue()
        {
            lock (_syncRoot)
            {
                if (_count == 0)
                    throw new InvalidOperationException("队列为空");

                byte result = _buffer[_head];
                _head = (_head + 1) % Capacity;
                _count--;
                return result;
            }
        }
        /// <summary>
        /// 出队指定数量的字节
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">数量必须大于0</exception>
        /// <exception cref="InvalidOperationException">队列中元素不足</exception>
        public byte[] Dequeue(int count)
        {
            lock (_syncRoot)
            {
                if (count <= 0)
                    throw new ArgumentOutOfRangeException(nameof(count), "数量必须大于0");
                if (count > _count)
                    throw new InvalidOperationException("队列中元素不足。");

                byte[] result = new byte[count];

                // 分两种情况复制数据
                int firstPart = Math.Min(Capacity - _head, count);
                Array.Copy(_buffer, _head, result, 0, firstPart);

                if (firstPart < count)
                {
                    int secondPart = count - firstPart;
                    Array.Copy(_buffer, 0, result, firstPart, secondPart);
                }

                // 更新头指针和计数
                _head = (_head + count) % Capacity;
                _count -= count;

                return result;
            }
        }
        /// <summary>
        /// 取出队列中所有字节并清空队列（若队列为空则返回空数组）
        /// </summary>
        public byte[] DequeueAll()
        {
            lock (_syncRoot)
            {
                if (_count == 0)
                    return new byte[] { };

                return Dequeue(_count);
            }
        }
        /// <summary>
        /// 尝试出队
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryDequeue(out byte result)
        {
            lock (_syncRoot)
            {
                if (_count == 0)
                {
                    result = default;
                    return false;
                }
                result = Dequeue();
                return true;
            }
        }
        /// <summary>
        /// 尝试取出指定数量的字节
        /// </summary>
        public bool TryDequeue(int count, out byte[] data)
        {
            lock (_syncRoot)
            {
                if (count <= 0 || _count == 0)
                {
                    data = new byte[] { };
                    return false;
                }

                int actualCount = Math.Min(count, _count);
                data = Dequeue(actualCount);
                return actualCount > 0;
            }
        }
        /// <summary>
        /// 查看队头元素
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">队列为空</exception>
        public byte Peek()
        {
            lock (_syncRoot)
            {
                if (_count == 0)
                    throw new InvalidOperationException("队列为空.");

                return _buffer[_head];
            }
        }
        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            lock (_syncRoot)
            {
                _head = _tail = _count = 0;
                Array.Clear(_buffer, 0, Capacity);
            }
        }
        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="value">元素</param>
        /// <returns></returns>
        public bool Contains(byte value)
        {
            lock (_syncRoot)
            {
                int index = _head;
                for (int i = 0; i < _count; i++)
                {
                    if (_buffer[index] == value)
                        return true;
                    index = (index + 1) % Capacity;
                }
                return false;
            }
        }
        /// <summary>
        /// 转换为数组
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            lock (_syncRoot)
            {
                byte[] result = new byte[_count];
                if (_count == 0) 
                    return result;

                if (_head < _tail)
                {
                    Array.Copy(_buffer, _head, result, 0, _count);
                }
                else
                {
                    int firstPart = Capacity - _head;
                    Array.Copy(_buffer, _head, result, 0, firstPart);
                    Array.Copy(_buffer, 0, result, firstPart, _tail);
                }
                return result;
            }
        }
    }
}
