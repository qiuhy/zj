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
        private long _count = 0;

        public Combination(int size, int maxLevel = 0)
        {
            _size = size;
            _maxLevel = maxLevel;
            _count = MathUtil.CombinationCount(_size, _maxLevel);
        }

        public long Count { get => _count; }

        public Object Current
        {
            get
            {
                int[] r = _stack.ToArray();
                Array.Reverse(r);
                return r;
            }
        }

        public void Reset() => _stack.Clear();
        public IEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (_size == 0 || _size < _maxLevel)
                return false;

            if (_stack.Count == 0) //初始值，[0,1,2,...]
            {
                for (int i = 0; i < _maxLevel; i++)
                {
                    _stack.Push(i);
                }
                return true;
            }

            return moveByLevel();
        }

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

        public int[] this[long idx] { get => (idx < 0 || idx >= _count) ? null : getResult(idx); }

        private int[] getResult(long idx)
        {
            int[] r = new int[_maxLevel];
            long remainCount = _count - idx - 1;
            int remainSize = _size;
            long cc = 0;
            for (int i = _maxLevel; i > 0; i--)
            {
                do
                {
                    remainSize--;
                    cc = MathUtil.CombinationCount(remainSize, i);
                } while (cc > remainCount);
                remainCount -= cc;
                r[_maxLevel - i] = _size - remainSize - 1;
            }
            return r;
        }
    }
}
