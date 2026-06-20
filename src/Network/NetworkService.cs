
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Chat.Client.V1;
using Chat.Server.V1;
using Godot;
using Google.Protobuf;

/// <summary>
/// 这个类用来处理网络连接，将远程服务器发来的数据转化为事件</br>
/// 并分发，和发送网络请求。
/// </summary>
public partial class NetworkService() : Node, IRequestSender, IEventDispatcher
{

    private readonly Queue<Event> _events = [];

    // 网络相关实现 //

    private ClientWebSocket _client = null;
    private CancellationTokenSource _cts;

    private async Task ReceiveLoop()
    {

        var token = _cts.Token;
        var buffer = new byte[4 * 1024];

        while (!token.IsCancellationRequested)
        {
            // 接收数据
            var result = await _client.ReceiveAsync(buffer, token);

            // 如果发送连接关闭，直接跳出循环，停止函数运行。
            if (result.CloseStatus != null)
            {
                break;
            }

            // 如果数据没发完？可能需要更好的处理方法。
            if (!result.EndOfMessage)
            {
                GD.Print("Received too much datas! closing client!");
                break;
            }

            // 如果数据类型不是二进制消息，直接跳过，读取下一条消息。
            if (result.MessageType != WebSocketMessageType.Binary)
            {
                continue;
            }

            // 解析 Protobuf 对象
            var serverEvent = Chat.Server.V1.ServerEvent.Parser.ParseFrom(buffer, 0, result.Count);

            // 将 Protobuf 对象转换成事件对象
            Event e = null;

            switch (serverEvent.DataCase)
            {

                case Chat.Server.V1.ServerEvent.DataOneofCase.None:
                    {
                        GD.Print("Received empty server event!");
                        break;
                    }

                case Chat.Server.V1.ServerEvent.DataOneofCase.Error:
                    {
                        var error = serverEvent.Error;
                        e = new ErrorEvent(serverEvent.Version, error.Code, error.Error);
                        break;
                    }

                case Chat.Server.V1.ServerEvent.DataOneofCase.Rooms:
                    {
                        var rooms = serverEvent.Rooms.Rooms.Select((room) => new RoomsEvent.RoomInfo(room.Num, room.Name, room.Owner, room.UserCount, room.MaxUserCount)).ToList();
                        e = new RoomsEvent(serverEvent.Version, rooms);
                        break;
                    }

                case Chat.Server.V1.ServerEvent.DataOneofCase.RoomJoined:
                    {
                        var roomJoined = serverEvent.RoomJoined;
                        var owner = new UserInfo(Guid.Parse(roomJoined.Owner.Id), roomJoined.Owner.Name);
                        var users = roomJoined.Users.Select((user) => new UserInfo(Guid.Parse(user.Id), user.Name)).ToList();
                        e = new RoomJoinedEvent(serverEvent.Version, roomJoined.Num, roomJoined.Name, owner, users);
                        break;
                    }

                case Chat.Server.V1.ServerEvent.DataOneofCase.RoomLeft:
                    {
                        e = new RoomLeftEvent(serverEvent.Version);
                        break;
                    }

                case Chat.Server.V1.ServerEvent.DataOneofCase.RoomUserJoined:
                    {
                        var userJoined = serverEvent.RoomUserJoined;
                        var userInfo = new UserInfo(Guid.Parse(userJoined.User.Id), userJoined.User.Name);
                        e = new UserJoinRoomEvent(serverEvent.Version, userInfo);
                        break;
                    }

                case Chat.Server.V1.ServerEvent.DataOneofCase.RoomUserLeft:
                    {
                        var userLeft = serverEvent.RoomUserLeft;
                        var userInfo = new UserInfo(Guid.Parse(userLeft.User.Id), userLeft.User.Name);
                        e = new UserLeftRoomEvent(serverEvent.Version, userInfo);
                        break;
                    }

                case Chat.Server.V1.ServerEvent.DataOneofCase.RoomGameStarted:
                    {
                        e = new GameStartEvent(serverEvent.Version, Guid.Parse(serverEvent.RoomGameStarted.PlayerId));
                        break;
                    }

                case Chat.Server.V1.ServerEvent.DataOneofCase.RoomGameStopped:
                    {
                        e = new GameStopEvent(serverEvent.Version);
                        break;
                    }

                case Chat.Server.V1.ServerEvent.DataOneofCase.ServerTick:
                    {
                        var serverTick = serverEvent.ServerTick;
                        var players = serverTick.Players.Select((player) => new ServerTickEvent.PlayerState(Guid.Parse(player.Id), player.Name, player.X, player.Y)).ToList();
                        var messages = serverTick.Messages.Select((message) => new ServerTickEvent.ChatMessage(Guid.Parse(message.SenderId), message.Message)).ToList();
                        e = new ServerTickEvent(serverEvent.Version, players, messages);
                        break;
                    }

            }

            if (e == null)
            {
                return;
            }

            // 将事件添加到队列中
            lock (_events)
            {
                _events.Enqueue(e);
            }

        }

        await DisconnectAsync();

    }

    public async Task SendAsync(byte[] bytes)
    {

        if (_client == null)
        {
            throw new InvalidOperationException("客户端未连接，无法发送数据。");
        }

        await _client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, _cts.Token);

    }

    private async Task ClearConnection()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _client?.Dispose();
        _client = null;
    }

    public async Task DisconnectAsync()
    {
        if (_client != null)
        {

            if (_client.State == WebSocketState.Open)
            {
                await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }

            await ClearConnection();
        }
    }

    public async Task ConnectAsync(Uri uri, string token)
    {

        await DisconnectAsync();

        _cts = new();
        _client = new();

        _client.Options.CollectHttpResponseDetails = true;
        _client.Options.SetRequestHeader("Authorization", "Bearer " + token);

        try
        {
            await _client.ConnectAsync(uri, _cts.Token);
            _ = ReceiveLoop();
        }
        catch (Exception)
        {
            GD.PrintErr("连接失败。 Code: ", _client.HttpStatusCode);
            await ClearConnection();
        }

        _events.Enqueue(new ConnectEvent());

    }

    // 事件分发器实现 //

    private readonly Dictionary<Type, List<EventSubscription>> _subs = [];
    private readonly Dictionary<IEventSubscriber, List<EventSubscription>> _subscribers = [];

    /// <summary>
    /// 添加订阅项的内部实现，已加锁。
    /// </summary>
    private void AddSubscription(EventSubscription sub, IEventSubscriber subscriber)
    {
        var t = sub.EventType;

        lock (_subs)
        {
            var ok = _subs.TryGetValue(t, out var subs);

            if (!ok)
            {
                subs = [];
                _subs[t] = subs;
            }

            if (!subs.Contains(sub))
            {
                subs.Add(sub);
            }
        }

        // 添加订阅者
        if (subscriber == null)
        {
            return;
        }

        lock (_subscribers)
        {
            var ok = _subscribers.TryGetValue(subscriber, out var subscriberSubs);

            if (!ok)
            {
                subscriberSubs = [];
                _subscribers[subscriber] = subscriberSubs;
            }

            if (!subscriberSubs.Contains(sub))
            {
                subscriberSubs.Add(sub);
            }
        }

    }

    private void RemoveSubscription(EventSubscription sub, IEventSubscriber subscriber)
    {
        var t = sub.EventType;

        lock (_subs)
        {
            var ok = _subs.TryGetValue(t, out var subs);

            if (!ok)
            {
                return;
            }

            subs.Remove(sub);
        }

        if (subscriber == null)
        {
            return;
        }

        lock (_subscribers)
        {
            var ok = _subscribers.TryGetValue(subscriber, out var subscriberSubs);

            if (!ok)
            {
                return;
            }

            subscriberSubs.Remove(sub);
        }

    }

    public void Subscribe<T>(EventSubscription<T> sub, IEventSubscriber subscriber = null) where T : Event
    {
        var type = typeof(T);
        AddSubscription(sub, subscriber);
    }

    public void Subscribe(IEventSubscriber subscriber)
    {
        var subs = subscriber.Subscriptions();

        foreach (var sub in subs)
        {
            if (!sub.EventType.IsAssignableTo(typeof(Event)))
            {
                continue;
            }

            AddSubscription(sub, subscriber);
        }
    }

    public void Unsubscribe<T>(EventSubscription<T> sub, IEventSubscriber subscriber = null) where T : Event
    {
        var type = typeof(T);
        RemoveSubscription(sub, subscriber);
    }

    public void Unsubscribe(IEventSubscriber subscriber)
    {
        var subs = new List<EventSubscription>();

        lock (_subscribers)
        {
            var ok = _subscribers.TryGetValue(subscriber, out var temp);

            if (!ok)
            {
                return;
            }

            subs = [.. temp];
        }

        foreach (var sub in subs)
        {
            RemoveSubscription(sub, subscriber);
        }

        lock (_subscribers)
        {
            _subscribers.Remove(subscriber);
        }
    }

    public void Dispatch<T>(T e) where T : Event
    {

        // GD.Print("Dispatch ", typeof(T).Name);

        List<EventSubscription> list = null;

        lock (_subs)
        {
            var ok = _subs.TryGetValue(typeof(T), out var temp);
            if (temp == null)
            {
                return;
            }

            list = [.. temp];
        }

        foreach (var sub in list)
        {

            if (sub is not EventSubscription<T> s)
            {
                continue;
            }

            var handled = s.Callback(s, e);
            if (handled)
            {
                break;
            }

        }

    }

    // 请求发送器实现 //

    public void SendRequest(ClientRequest request)
    {
        if (_client == null)
        {
            return;
        }

        request.Version = ApiVersion.Version;
        _ = SendAsync(request.ToByteArray());
    }

    // 节点方法 //

    public override void _Process(double delta)
    {
        // 在主线程分发事件
        lock (_events)
        {
            while (_events.TryDequeue(out var e))
            {
                e.DispatchTo(this);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // 在主线程分发事件
        lock (_events)
        {
            while (_events.TryDequeue(out var e))
            {
                e.DispatchTo(this);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts?.Cancel();
            _cts?.Dispose();

            _client?.Dispose();
        }

        base.Dispose(disposing);
    }

}
