
using System;

/// <returns>事件是否已被处理，或是否打断事件处理流程？</returns>
public delegate bool EventCallback<T>(EventSubscription<T> subscription, T e) where T : Event;

public class EventSubscription
{
}

public class EventSubscription<T>(
    EventCallback<T> callback
) : EventSubscription where T : Event
{
    public EventCallback<T> Callback { get; } = callback;
}

public interface IEventDispatcher
{
    public void Subscribe<T>(EventSubscription<T> subscription) where T : Event;
    public void Subscribe<T>(EventCallback<T> callback) where T : Event;

    public void Unsubscribe<T>(EventSubscription<T> subscription) where T : Event;

    public void Dispatch<T>(T e) where T : Event;
}
