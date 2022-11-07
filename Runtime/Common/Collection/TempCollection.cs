﻿using System;
using System.Collections.Generic;

namespace Pancake
{
    public interface ITempCollection<T> : ICollection<T>, IDisposable
    {
    }

    /// <summary>
    /// This is intended for a short lived collection that needs to be memory efficient and fast. 
    /// Call the static 'GetCollection' method to get a cached collection for use. 
    /// When you're done with the collection you call Release to make it available for reuse again later. 
    /// Do NOT use it again after calling Release.
    /// 
    /// Due to the design of this, it should only ever be used in a single threaded manner. Primarily intended 
    /// for the main Unity thread. 
    /// 
    /// If you're in a separate thread, it's best to cache your own list local to there, and don't even bother with 
    /// this.
    /// </summary>
    public static class TempCollection
    {
        #region Static Interface

        /// <summary>
        /// Returns the any available collection for use generically. 
        /// The collection could be a HashSet, List, or any temp implementation. 
        /// This is intended to reduce the need for creating a new collection 
        /// unnecessarily.
        /// </summary>
        /// <returns></returns>
        public static ITempCollection<T> GetCollection<T>() { return GetList<T>(); }

        /// <summary>
        /// Returns the any available collection for use generically. 
        /// The collection could be a HashSet, List, or any temp implementation. 
        /// This is intended to reduce the need for creating a new collection 
        /// unnecessarily.
        /// </summary>
        /// <returns></returns>
        public static ITempCollection<T> GetCollection<T>(IEnumerable<T> e) { return GetList<T>(e); }
        
        public static TempList<T> GetList<T>() { return TempList<T>.Get(); }

        public static TempList<T> GetList<T>(IEnumerable<T> e) { return TempList<T>.Get(e); }

        public static TempList<T> GetList<T>(int count) { return TempList<T>.Get(count); }

        public static TempHashSet<T> GetHashSet<T>() { return TempHashSet<T>.Get(); }

        public static TempHashSet<T> GetHashSet<T>(IEqualityComparer<T> comparer) { return TempHashSet<T>.Get(comparer); }

        public static TempHashSet<T> GetHashSet<T>(IEnumerable<T> e) { return TempHashSet<T>.Get(e); }

        public static TempHashSet<T> GetHashSet<T>(IEnumerable<T> e, IEqualityComparer<T> comparer) { return TempHashSet<T>.Get(e, comparer); }

        public static TempDictionary<TKey, TValue> GetDictionary<TKey, TValue>() { return TempDictionary<TKey, TValue>.Get(); }

        public static TempDictionary<TKey, TValue> GetDictionary<TKey, TValue>(IEqualityComparer<TKey> comparer) { return TempDictionary<TKey, TValue>.Get(comparer); }

        public static TempDictionary<TKey, TValue> GetDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict) { return TempDictionary<TKey, TValue>.Get(dict); }

        public static TempDictionary<TKey, TValue> GetDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict, IEqualityComparer<TKey> comparer)
        {
            return TempDictionary<TKey, TValue>.Get(dict, comparer);
        }

        public static TempQueue<T> GetQueue<T>() { return TempQueue<T>.Get(); }

        public static TempQueue<T> GetQueue<T>(IEnumerable<T> e) { return TempQueue<T>.Get(e); }

        public static TempCallbackCollection<T> GetCallbackCollection<T>(Action<T> addCallback, Action<T> removeCallback = null)
        {
            return TempCallbackCollection<T>.Get(addCallback, removeCallback);
        }

        #endregion
    }
}