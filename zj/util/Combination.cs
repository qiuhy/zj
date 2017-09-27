using System;
using System.Collections;
using System.Collections.Generic;
namespace util
{
    class Combination : IEnumerable, IEnumerator
    {
        private Stack<int> _stack = new Stack<int>();
        private int _size;
        private int _maxLevel;
        private bool _allowAllLevel = true;
        private bool _orderByLevel = true;
        private long _count = 0;

        public Combination(int size, int maxLevel = 0, bool allowAllLevel = true, bool orderByLevel = true)
        {
            _size = size;
            _maxLevel = maxLevel == 0 || maxLevel > size ? size : maxLevel;
            _allowAllLevel = allowAllLevel;
            _orderByLevel = orderByLevel;
            _count = Math.CombinationCount(_size, _maxLevel, _allowAllLevel);
        }

        public long Count { get => _count; }

        public Object Current
        {
            get
            {
                int[] r = new int[_stack.Count];
                int i = r.Length;
                foreach (int e in _stack)
                    r[--i] = e;
                return r;
            }
        }

        public void Reset() => _stack.Clear();
        public IEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (_size == 0)
                return false;
            if (!_allowAllLevel && _size < _maxLevel)
                return false;

            if (_stack.Count == 0) //初始值，[0]或 [0,1,2,...]
            {
                _stack.Push(0);
                if (!_allowAllLevel)
                    for (int i = 1; i < _maxLevel; i++)
                    {
                        _stack.Push(i);
                    }
                return true;
            }

            return _orderByLevel ? moveByLevel() : moveByIndex();
        }

        //顺序从小到大 c(3,3) = [0],[0,1],[0,1,2],[0,2],[1],[1,2],[2]
        private bool moveByIndex()
        {
            int e = (_stack.Count == _maxLevel) ? _stack.Pop() : _stack.Peek();
            while (++e == _size)
            {
                if (_stack.Count == 0)
                    return false;
                e = _stack.Pop();
            }
            _stack.Push(e);
            if (_allowAllLevel || _stack.Count == _maxLevel)
                return true;
            return moveByIndex();
        }

        //数量从小到大 c(3,3) = [0],[1],[2],[0,1],[0,2],[1,2],[0,1,2]
        private bool moveByLevel()
        {
            int count = _stack.Count;

            int e = _stack.Pop();
            if (++e == _size)
            {
                e--;
                while (_stack.Count > 0)
                {
                    int e0 = _stack.Pop();
                    if (e0 != --e)
                    {
                        e = e0 + 1;
                        for (int i = _stack.Count; i < count; i++)
                            _stack.Push(e++);
                        return true;
                    }
                }
                if (count == _maxLevel)
                    return false;

                for (e = 0; e < count; e++)
                    _stack.Push(e);
            }
            _stack.Push(e);
            return true;
        }

        public int[] this[long idx]
        {
            get
            {
                if (idx < 0 || idx >= _count)
                    return null;

                if (_allowAllLevel)
                {
                    if (_orderByLevel)
                    {
                        int level = 0;
                        long count = 0;
                        while (idx >= count)
                        {
                            idx -= count;
                            level++;
                            count = Math.CombinationCount(_size, level);
                        }
                        return getResult(idx, count, level);
                    }
                    else
                    {
                        // TODO 按照数字大小排序 的指定位置 的值
                        List<int> r = new List<int>();
                        return r.ToArray();
                    }
                }
                return getResult(idx, _count, _maxLevel);
            }
        }
        private int[] getResult(long idx, long count, int level)
        {
            int[] r = new int[level];
            long remainCount = count - idx - 1;
            int remainSize = _size;
            long cc = 0;
            for (int i = level; i > 0; i--)
            {
                do
                {
                    remainSize--;
                    cc = Math.CombinationCount(remainSize, i);
                } while (cc > remainCount);
                remainCount -= cc;
                r[level - i] = _size - remainSize - 1;
            }
            return r;
        }
    }
}
