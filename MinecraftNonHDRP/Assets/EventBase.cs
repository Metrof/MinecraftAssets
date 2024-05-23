using System;
using System.Collections.Generic;

public static class EventBase
{
    private static Dictionary<string, List<object>> _signalCallbacks = new();

    public static void Subscribe<T>(Action<T> callback)
    {
        string key = typeof(T).Name;
        if (_signalCallbacks.ContainsKey(key))
        {
            _signalCallbacks[key].Add(callback);
        }
        else
        {
            _signalCallbacks.Add(key, new List<object>());
            _signalCallbacks[key].Add(callback);
        }
    }

    public static void Unsubscribe<T>()
    {
        string key = typeof(T).Name;
        if (_signalCallbacks.ContainsKey(key))
        {
            _signalCallbacks.Remove(key);
        }
    }

    public static void Invoke<T>(T signal)
    {
        string key = typeof(T).Name;
        if (_signalCallbacks.ContainsKey(key))
        {
            foreach (var obj in _signalCallbacks[key])
            {
                var callback = obj as Action<T>;
                callback?.Invoke(signal);
            }
        }
    }
}
