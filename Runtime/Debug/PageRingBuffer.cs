#if UNITY_EDITOR
using System;
using System.Runtime.CompilerServices;

namespace FFS.Libraries.StaticEcs.Unity {
    public struct Page {
        internal long PageId;
        internal int Count;

        public Page(long pageId) {
            PageId = pageId;
            Count = 0;
        }

        public void Reset(long newPageId) {
            Count = 0;
            PageId = newPageId;
        }
    }

    public struct PageView<T> where T : IEquatable<T> {
        internal readonly PageRingBuffer<T> Parent;
        public long PageId;
        public int Index;

        public PageView(long pageId, PageRingBuffer<T> parent, int index) {
            PageId = pageId;
            Parent = parent;
            Index = index;
        }

        public ref T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Parent.Data[Index, index];
        }

        public bool IsActual => Parent.Pages[Index].PageId == PageId;
        public ref int Count => ref Parent.Pages[Index].Count;
        public int PageSize => Parent.PageSize;
        public PageViewEnumerator<T> GetEnumerator() => new(this);

        public bool HasNewer => PageId + 1 <= Parent.LastPage && Parent.GetPageView(PageId + 1).Count > 0;
        public bool HasOlder => PageId - 1 >= Parent.FirstPage;

        public void MoveToNewer() {
            var page = Parent.GetPageView(PageId + 1);
            PageId = page.PageId;
            Index = page.Index;
        }

        public void MoveToOlder() {
            var page = Parent.GetPageView(PageId - 1);
            PageId = page.PageId;
            Index = page.Index;
        }

        public PageView<T> GetOlder() {
            return Parent.GetPageView(PageId - 1);
        }
    }

    public ref struct PageViewEnumerator<T> where T : IEquatable<T> {
        internal readonly PageView<T> View;
        internal int Count;

        public PageViewEnumerator(PageView<T> view) {
            View = view;
            Count = view.Count;
        }

        public ref T Current {
            get => ref View[Count];
        }

        public bool MoveNext() {
            if (Count == 0) {
                return false;
            }

            Count--;
            return true;
        }
    }

    public sealed class PageRingBuffer<T> where T : IEquatable<T> {
        internal readonly T[,] Data;
        internal readonly Page[] Pages;
        private int count;
        private int Head;
        public readonly int PageSize;
        private long MaxPageId;
        private long MinPageId;
        private OnPush _onPush;
        private OnChange _onChange;

        public delegate void OnPush(ref T item);

        public delegate void OnChange(ref T item);

        public delegate void Replace(T template, ref T item);

        public PageRingBuffer(int pageCount, int pageSize = 16) {
            PageSize = pageSize;
            Data = new T[pageCount, pageSize];
            Pages = new Page[pageCount];
            for (var i = 0; i < Pages.Length; i++) {
                Pages[i] = new Page(i);
            }

            MaxPageId = pageCount;
            MinPageId = 0;
            Head = 0;
            count = 0;
        }

        public void SetOnPush(OnPush onPush) {
            _onPush = onPush;
        }

        public void SetOnChange(OnChange onChange) {
            _onChange = onChange;
        }

        public void Change(T template, Replace replace) {
            for (var i = 0; i < count; i++) {
                if (template.Equals(this[i])) {
                    replace(template, ref this[i]);
                    _onChange?.Invoke(ref this[i]);
                    return;
                }
            }
        }

        public int Capacity => Pages.Length * PageSize;
        public bool IsFull => Capacity == Count;
        public int Count => count;
        public int PageCount => Pages.Length;

        public ref long FirstPage => ref MinPageId;
        public long LastPage => IsFull ? MaxPageId - 1 : Math.Max(count - 1, 0) / PageSize;


        public PageView<T> GetPageView(uint page) {
            var idx = (MinPageId + page) % PageSize;
            ref var val = ref Pages[(int) idx];
            return new PageView<T>(val.PageId, this, (int) idx);
        }

        public PageView<T> GetPageView(long pageId) {
            var idx = pageId % PageCount;
            ref var val = ref Pages[(int) idx];
            return new PageView<T>(val.PageId, this, (int) idx);
        }

        public void Push(T item) {
            Data[Head / PageSize, Pages[Head / PageSize].Count++] = item;
            _onPush?.Invoke(ref item);
            Head++;
            Head %= Capacity;
            if (!IsFull) {
                count++;
            }

            if (IsFull && Head % PageSize == 0) {
                Pages[Head / PageSize].Reset(MaxPageId++);
                MinPageId++;
            }
        }

        public ref T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Data[index / PageSize, index % PageSize];
        }

        public void Reset() {
            for (var i = 0; i < Pages.Length; i++) {
                Pages[i].Reset(i);
            }

            MaxPageId = Pages.Length;
            MinPageId = 0;
            Head = 0;
            count = 0;
        }
    }
}
#endif