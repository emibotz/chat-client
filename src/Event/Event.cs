

using System;
using System.Collections;
using System.Collections.Generic;

/// <returns>是否打断事件处理过程</returns>
public delegate bool EventCallback<T>(EventSubscription<T> sub, T e) where T : Event;

public abstract class EventSubscription
{
    public abstract Type EventType { get; }
}

public class EventSubscription<T>(
    EventCallback<T> callback
) : EventSubscription where T : Event
{
    public override Type EventType { get => typeof(T); }
    public EventCallback<T> Callback { get; } = callback;
}

public class EventSubscriptions : IEnumerable<EventSubscription>
{
    private readonly List<EventSubscription> _subs = [];

    public void Add(EventSubscription sub) => _subs.Add(sub);

    public IEnumerator<EventSubscription> GetEnumerator() => _subs.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public interface IEventSubscriber
{
    public EventSubscriptions Subscriptions();
}

public interface IEventDispatcher
{
    public void Subscribe<T>(EventSubscription<T> subscription, IEventSubscriber subscriber = null) where T : Event;
    public void Subscribe(IEventSubscriber subscriber);

    public void Unsubscribe<T>(EventSubscription<T> subscription, IEventSubscriber subscriber = null) where T : Event;
    public void Unsubscribe(IEventSubscriber subscriber);


    public void Dispatch<T>(T e) where T : Event;
}
