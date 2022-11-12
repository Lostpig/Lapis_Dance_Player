using System;
using System.Collections.Generic;
using System.Linq;
namespace LapisPlayer
{
    internal class PlayerGlobal
    {
        static private PlayerGlobal _instance;
        static public PlayerGlobal Instance
        {
            get
            {
                if (_instance == null) _instance = new PlayerGlobal();
                return _instance;
            }
        }

        Dictionary<Type, Object> _dict = new();
        public void SetSingleton<T>(T instance)
        {
            _dict[typeof(T)] = instance;
        }
        public T GetSingleton<T>()
        {
            var key = typeof(T);
            _dict.TryGetValue(key, out var value);
            return (T)value;
        }
    }
}
