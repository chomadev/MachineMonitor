using System;
using System.Collections.Generic;
using SystemChecker.Core.Interfaces;

namespace SystemChecker.Core.Services;

public class MessagingCenter : IMessagingCenter
{
    private static readonly Dictionary<string, List<Action<object>>> _subscribers 
        = new Dictionary<string, List<Action<object>>>();

    public void Subscribe<T>(object subscriber, string message, Action<T> callback)
    {
        var key = message;
        if (!_subscribers.ContainsKey(key))
        {
            _subscribers[key] = new List<Action<object>>();
        }

        _subscribers[key].Add((obj) => callback((T)obj));
    }

    public void Publish<T>(T message, string messageKey)
    {
        var key = messageKey;
        if (_subscribers.ContainsKey(key))
        {
            foreach (var subscriber in _subscribers[key].ToList())
            {
                subscriber(message);
            }
        }
    }
} 