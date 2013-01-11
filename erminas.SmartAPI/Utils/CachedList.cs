/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace erminas.SmartAPI.Utils
{
    public class CachedList<T> : ICachedList<T> where T : class
    {
        private bool _isCachingEnabled;

        public CachedList(Func<List<T>> retrieveFunc, Caching caching)
        {
            RetrieveFunc = retrieveFunc;
            _isCachingEnabled = caching == Caching.Enabled;
        }

        protected CachedList(Caching caching)
        {
            _isCachingEnabled = caching == Caching.Enabled;
        }

        protected Func<List<T>> RetrieveFunc { private get;  set; }

        protected virtual List<T> List { get; set; }

        #region ICachedList<T> Members

        public T GetByPosition(int pos)
        {
            CheckList();
            return List[pos];
        }

        public IEnumerator<T> GetEnumerator()
        {
            CheckList();
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            CheckList();
            return List.GetEnumerator();
        }

        public virtual bool IsCachingEnabled
        {
            get { return _isCachingEnabled; }
            set { _isCachingEnabled = value; }
        }

        public void Refresh()
        {
            List = null;
            if (IsCachingEnabled)
            {
                CheckList();
            }
        }

        public void InvalidateCache()
        {
            List = null;
        }

        #endregion

        protected void CheckList()
        {
            if (IsCachingEnabled && List != null)
            {
                return;
            }

            List = RetrieveFunc();
        }
    }

    public enum Caching
    {
        Enabled,
        Disabled
    }
}