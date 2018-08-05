using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Collections;

namespace OCR.Classes
{
    public class CacheManager
    {
        private CacheManager()
        {
            //Make Singleton
        }
        private static CacheManager _mgr;
        private static Dictionary<string, object> cache = new Dictionary<string, object>();
        private static readonly object _obj = new object();

        public static CacheManager GetInstance()
        {
            lock (_obj)
            {
                if (_mgr == null)
                {
                    _mgr = new CacheManager();
                }
                return _mgr;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public void SetItem(string Key, object Value)
        {
            lock (_obj)
            {
                if (!cache.ContainsKey(Key))
                {
                    cache.Add(Key, Value);
                }
                else
                {
                    cache[Key] = Value;
                }
                    
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public object GetItem(string Key)
        {
            lock (_obj)
            {
                if (cache.ContainsKey(Key))
                {
                    return cache[Key];
                }
                else
                {
                    return null;
                }
            }
        }
    }
}