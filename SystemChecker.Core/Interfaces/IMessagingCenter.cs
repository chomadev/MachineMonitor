using System;

namespace SystemChecker.Core.Interfaces;

public interface IMessagingCenter
{
    void Subscribe<T>(object subscriber, string message, Action<T> callback);
    void Publish<T>(T message, string messageKey);
} 