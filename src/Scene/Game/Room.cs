
using System;
using System.Collections.Generic;
using Chat.Server.V1;

public record User(Guid Id, string Name)
{
    public User(string id, string name) : this(Guid.Parse(id), name) { }
    public User(UserInfo u) : this(u.Id, u.Name) { }
}

public class Room(long num, string name, User owner)
{
    public long Num { get; private set; } = num;
    public string Name { get; private set; } = name;

    private readonly User _owner = owner;
    public User Owner
    {
        get => _owner;
    }

    private readonly List<User> _users = [owner];
    public IReadOnlyList<User> Users { get => _users; }

    public void AddUser(User u)
    {
        if (_users.Contains(u))
        {
            return;
        }
        _users.Add(u);
    }

    public void RemoveUser(User u)
    {
        _users.Remove(u);
    }

    public void RemoveUser(Guid id)
    {
        if (_users.Find((u) => u.Id == id) is User u)
        {
            RemoveUser(u);
        }
    }

    public Room(long num, string name, UserInfo owner, IReadOnlyList<UserInfo> users) : this(num, name, new User(owner))
    {
        foreach (var user in users)
        {
            AddUser(new User(user));
        }
    }
}
