
using System;
using System.Collections.Generic;

public abstract class Event
{
    public abstract void DispatchTo(IEventDispatcher dispatcher);
}

public class ConnectEvent() : Event
{
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class DisconnectEvent() : Event
{
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public abstract class ServerEvent(
    string apiVersion
) : Event
{
    public string ApiVersion { get; } = apiVersion;
}

public class ErrorEvent(
    string apiVersion,
    int code,
    string error
) : ServerEvent(apiVersion)
{
    public int Code { get; } = code;
    public string Error { get; } = error;
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class RoomsEvent(
    string apiVersion,
    IReadOnlyList<RoomsEvent.RoomInfo> rooms
) : ServerEvent(apiVersion)
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
    Guid id,
    string name
)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
}

public class RoomJoinedEvent(
    string apiVersion,
    long num,
    string name,
    UserInfo owner,
    IReadOnlyList<UserInfo> users
) : ServerEvent(apiVersion)
{
    public long Num { get; } = num;
    public string Name { get; } = name;
    public UserInfo Owner { get; } = owner;
    public IReadOnlyList<UserInfo> Users { get; } = users;
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class RoomLeftEvent(
    string apiVersion
) : ServerEvent(apiVersion)
{
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class UserJoinRoomEvent(
    string apiVersion,
    UserInfo user
) : ServerEvent(apiVersion)
{
    public UserInfo User { get; } = user;
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class UserLeftRoomEvent(
    string apiVersion,
    UserInfo user
) : ServerEvent(apiVersion)
{
    public UserInfo User { get; } = user;
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class GameStartEvent(
    string apiVersion,
    Guid playerId
) : ServerEvent(apiVersion)
{
    public Guid PlayerId { get; } = playerId;
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}

public class GameStopEvent(
    string apiVersion
) : ServerEvent(apiVersion)
{
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}
public class ServerTickEvent(
    string apiVersion,
    IReadOnlyList<ServerTickEvent.PlayerState> players,
    IReadOnlyList<ServerTickEvent.ChatMessage> messages
) : ServerEvent(apiVersion)
{
    public class PlayerState(
        Guid id,
        string name,
        double x,
        double y
    )
    {
        public Guid Id { get; } = id;
        public string Name { get; } = name;
        public double X { get; } = x;
        public double Y { get; } = y;
    }

    public class ChatMessage(
        Guid senderId,
        string message
    )
    {
        public Guid SenderId { get; } = senderId;
        public string Message { get; } = message;
    }

    public IReadOnlyList<PlayerState> Players { get; } = players;
    public IReadOnlyList<ChatMessage> Messages { get; } = messages;
    public override void DispatchTo(IEventDispatcher dispatcher) => dispatcher.Dispatch(this);
}
