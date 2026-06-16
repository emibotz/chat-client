
using System.Collections.Generic;

public class Event(
    string apiVersion
)
{
    public string ApiVersion { get; } = apiVersion;
    public virtual void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class ErrorEvent(
    string apiVersion,
    int code,
    string error
) : Event(apiVersion)
{
    public int Code { get; } = code;
    public string Error { get; } = error;
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class RoomsEvent(
    string apiVersion,
    IReadOnlyList<RoomsEvent.RoomInfo> rooms
) : Event(apiVersion)
{
    public class RoomInfo(
        long num,
        string name,
        string owner,
        int userCount,
        int maxUserCount
    )
    {
        public long Num { get; } = num;
        public string Name { get; } = name;
        public string Owner { get; } = owner;
        public int UserCount { get; } = userCount;
        public int MaxUserCount { get; } = maxUserCount;
    }

    public IReadOnlyList<RoomInfo> Rooms { get; } = rooms;
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class UserInfo(
    string id,
    string name
)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
}

public class RoomJoinedEvent(
    string apiVersion,
    long num,
    string name,
    UserInfo owner,
    IReadOnlyList<UserInfo> users
) : Event(apiVersion)
{
    public long Num { get; } = num;
    public string Name { get; } = name;
    public UserInfo Owner { get; } = owner;
    public IReadOnlyList<UserInfo> Users { get; } = users;
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class RoomLeftEvent(
    string apiVersion
) : Event(apiVersion)
{
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class UserJoinedRoomEvent(
    string apiVersion,
    UserInfo user
) : Event(apiVersion)
{
    public UserInfo User { get; } = user;
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class UserLeftRoomEvent(
    string apiVersion,
    UserInfo user
) : Event(apiVersion)
{
    public UserInfo User { get; } = user;
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class GameStartEvent(
    string apiVersion
) : Event(apiVersion)
{
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class GameStopEvent(
    string apiVersion
) : Event(apiVersion)
{
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}
public class ServerTickEvent(
    string apiVersion,
    IReadOnlyList<ServerTickEvent.PlayerState> players,
    IReadOnlyList<ServerTickEvent.ChatMessage> messages
) : Event(apiVersion)
{
    public class PlayerState(
        string id,
        string name,
        double x,
        double y
    )
    {
        public string Id { get; } = id;
        public string Name { get; } = name;
        public double X { get; } = x;
        public double Y { get; } = y;
    }

    public class ChatMessage(
        string senderId,
        string message
    )
    {
        public string SenderId { get; } = senderId;
        public string Message { get; } = message;
    }

    public IReadOnlyList<PlayerState> Players { get; } = players;
    public IReadOnlyList<ChatMessage> Messages { get; } = messages;
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}
