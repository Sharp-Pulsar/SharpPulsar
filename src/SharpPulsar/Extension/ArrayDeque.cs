
/*
 * This file is licensed under the MIT License (MIT).
 *
 * Copyright (c) JCThePants (github.com/JCThePants)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
 
using System;
using System.Collections.Generic;
using System.Collections;

namespace SharpPulsar.Extension
{

    public class ArrayDeque<T> : IList<T>
    {

        private T[] _array;
        private int _size = 0;

        // offset from beginning of array where front element is located
        private int _frontIndex = 0;

        /**
         * Constructor.
         *
         * <p>Initial capacity of 15.</p>
         */
        public ArrayDeque() : this(15)
        {
        }

        /**
         * Constructor.
         *
         * @param capacity  The initial capacity.
         */
        public ArrayDeque(int capacity)
        {
            _array = new T[capacity];
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public int Count
        {
            get { return _size; }
        }

        public T First
        {
            get
            {
                if (Count == 0)
                    throw new InvalidOperationException("There are no items.");

                return _array[_frontIndex];
            }
            set
            {
                if (Count == 0)
                {
                    AddFirst(value);
                }
                else
                {
                    _array[_frontIndex] = value;
                }
            }
        }

        public T Last
        {
            get
            {
                if (Count == 0)
                    throw new InvalidOperationException("There are no items.");

                return _array[TranslateIndex(_size - 1)];
            }
            set
            {
                if (Count == 0)
                {
                    AddLast(value);
                }
                else
                {
                    _array[TranslateIndex(_size - 1)] = value;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index > _size - 1)
                    throw new IndexOutOfRangeException();

                return _array[TranslateIndex(index)];
            }

            set
            {
                if (index < 0 || index > _size - 1)
                    throw new IndexOutOfRangeException();

                _array[TranslateIndex(index)] = value;
            }
        }

        public int IndexOf(T item)
        {

            var enumer = new Enumer(this);
            while (enumer.MoveNext())
            {
                if (item == null && enumer.Current == null)
                    return enumer.Index;

                if (item == null && enumer.Current != null)
                    continue;

                if (item != null && enumer.Current == null)
                    continue;

                if (item.Equals(enumer.Current))
                    return enumer.Index;
            }
            return -1;
        }

        public bool Contains(T item)
        {

            for (int i = 0; i < _size; i++)
            {

                int index = TranslateIndex(i);

                if (item == null)
                {

                    if (_array[index] == null)
                        return true;

                    continue;
                }

                if (item.Equals(_array[index]))
                    return true;
            }
            return false;
        }

        public bool ContainsAll(ICollection<T> collection)
        {

            foreach (T item in collection)
            {
                if (!Contains(item))
                    return false;
            }

            return true;
        }

        public void Add(T item)
        {
            AddLast(item);
        }

        public bool AddFirst(T item)
        {

            _size++;

            if (_size > _array.Length)
                ExpandCapacity();

            _frontIndex = NegMod(_frontIndex - 1, _array.Length);

            _array[_frontIndex] = item;

            return true;
        }

        public bool AddLast(T item)
        {

            _size++;

            if (_size > _array.Length)
                ExpandCapacity();

            int index = TranslateIndex(_size - 1);

            _array[index] = item;

            return true;
        }

        public bool AddRange(ICollection<T> collection)
        {

            EnsureCapacity(_size + collection.Count);

            foreach (T element in collection)
            {
                AddLast(element);
            }

            return true;
        }

        public void Insert(int index, T item)
        {

            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException();

            _size++;
            if (_size > _array.Length)
                ExpandCapacity();

            T buffer = item;
            for (int i = index; i < _size; i++)
            {

                int tIndex = TranslateIndex(i);
                T current = _array[tIndex];

                _array[tIndex] = buffer;
                buffer = current;
            }
        }

        public void RemoveAt(int index)
        {

            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException();

            for (int i = index; i < _size; i++)
            {

                int tIndex = TranslateIndex(i);

                if (i < _size - 1)
                {
                    int nxtIndex = TranslateIndex(i + 1);
                    _array[tIndex] = _array[nxtIndex];
                }
                else
                {
                    _array[tIndex] = default(T);
                }
            }

            _size--;
        }

        public bool Remove(T item)
        {

            bool isCompacting = false;

            for (int i = 0; i < _size; i++)
            {

                int index = TranslateIndex(i);

                if (!isCompacting)
                {

                    if (item == null)
                    {

                        if (_array[index] == null)
                        {
                            isCompacting = true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (item.Equals(_array[index]))
                    {
                        isCompacting = true;
                    }
                }

                if (isCompacting)
                {

                    _array[index] = i < _size - 1
                            ? _array[TranslateIndex(i + 1)]
                            : default(T);
                }
            }

            if (isCompacting)
            {
                _size--;

                if (Count == 0)
                    _frontIndex = 0;

                return true;
            }

            return false;
        }

        public T RemoveFirst()
        {

            if (Count == 0)
                //throw new InvalidOperationException("There are no items to remove.");
                return default(T);  

            _size--;

            T element = _array[_frontIndex];
            _array[_frontIndex] = default(T);

            _frontIndex = Count == 0 ? 0 : (_frontIndex + 1) % _array.Length;

            return element;
        }

        public T RemoveLast()
        {

            if (Count == 0)
                //throw new InvalidOperationException("There are no items to remove.");
                return default(T);

            _size--;

            int index = TranslateIndex(_size - 1);
            T element = _array[index];
            _array[index] = default(T);

            return element;
        }

        public bool RemoveAll(ICollection<T> collection)
        {

            bool isModified = false;

            foreach (T obj in collection)
            {
                isModified = Remove(obj) || isModified;
            }

            return isModified;
        }

        public bool RetainAll(ICollection<T> collection)
        {

            bool isModified = false;

            var enumer = new Enumer(this);
            while (enumer.MoveNext())
            {
                T element = enumer.Current;

                if (!collection.Contains(element))
                {
                    enumer.Remove();
                    isModified = true;
                }
            }

            if (Count == 0)
                _frontIndex = 0;

            return isModified;
        }

        public void Clear()
        {
            for (int i = 0; i < _array.Length; i++)
            {
                _array[i] = default(T);
            }
            _size = 0;
            _frontIndex = 0;
        }

        public T[] ToArray()
        {

            T[] array = new T[_size];
            CopyTo(array, 0);
            return array;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {

            var enumer = new Enumer(this);
            int index = arrayIndex;
            while (enumer.MoveNext())
            {
                array[index] = enumer.Current;
                index++;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumer(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        private int TranslateIndex(int index)
        {
            return (index + _frontIndex) % _array.Length;
        }

        private int NegMod(int index, int range)
        {
            int result = index % range;
            return result < 0 ? result + range : result;
        }

        private void ExpandCapacity()
        {

            int newSize = _array.Length + (int)Math.Max(10, (_array.Length * 0.2));
            EnsureCapacity(newSize);
        }

        private void EnsureCapacity(int size)
        {

            if (_array.Length >= size)
                return;

            T[] newArray = new T[size];

            int index = 0;
            var enumer = new Enumer(this);
            while (enumer.MoveNext())
            {
                T element = enumer.Current;

                newArray[index] = element;
                index++;
            }

            _frontIndex = 0;
            _array = newArray;
        }


        private class Enumer : IEnumerator<T>
        {

            private int index = -1;
            private bool nextInvoked;
            private ArrayDeque<T> parent;

            public Enumer(ArrayDeque<T> parent)
            {
                this.parent = parent;
            }

            public int Index
            {
                get { return index; }
            }

            public T Current
            {
                get
                {
                    try
                    {
                        return parent._array[parent.TranslateIndex(index)];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
                // do nothing
            }

            public bool MoveNext()
            {
                index++;
                nextInvoked = true;
                return index < parent._size;
            }

            public void Reset()
            {
                index = -1;
            }

            public void Remove()
            {

                if (!nextInvoked)
                    throw new InvalidOperationException("MoveNext must be invoked before Remove.");

                nextInvoked = false;

                parent._array[parent.TranslateIndex(index)] = default(T);

                for (int i = index; i < parent._size; i++)
                {

                    parent._array[parent.TranslateIndex(i)] = i < parent._size - 1
                            ? parent._array[parent.TranslateIndex(i + 1)]
                            : default(T);
                }

                index--;
                parent._size--;
            }
        }

    }
}