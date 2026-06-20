using Chat.Client.V1;
using Godot;
using System;

public partial class RoomMenu : VBoxContainer, IEventSubscriber
{

    // 房间实例，应该被父节点注入
    private Room _room;
    public Room Room
    {
        set
        {
            _room = value;
        }
    }

    public IRequestSender RequestSender { private get; set; }

    // 事件分发器，应该被外部注入
    private IEventDispatcher _eventDispatcher;
    public IEventDispatcher EventDispatcher
    {
        set
        {
            _eventDispatcher?.Unsubscribe(this);
            _eventDispatcher = value;
            _eventDispatcher?.Subscribe(this);
        }
    }

    // 场景节点 //

    private Tree _userList;
    private Label _roomInfo;
    private Button _startButton;
    private Button _leaveButton;

    // 功能方法 //

    // 刷新节点
    public void Refresh()
    {
        // 这里应该有一个根据权限决定是否显示
        // 开始按钮，但是没有实现权限服务，我
        // 懒了。

        // 刷新房间信息
        _roomInfo.Text = string.Format("房间号：{0}\n房间名：{1}", _room.Num.ToString().PadLeft(6, '0'), _room.Name);

        // 刷新用户列表
        _userList.Clear();
        _userList.CreateItem();

        if (_room == null)
        {
            return;
        }

        foreach (var user in _room.Users)
        {
            var item = _userList.CreateItem();

            item.SetTextAlignment(0, HorizontalAlignment.Left);
            item.SetText(0, user.Name);

            item.SetTextAlignment(1, HorizontalAlignment.Center);
            if (user == _room.Owner)
            {
                item.SetText(1, "✔");
            }
        }
    }

    // 信号处理 //

    private void OnStartButtonPressed()
    {
        var req = new ClientRequest()
        {
            StartGame = new RoomStartGame()
            {

            }
        };

        RequestSender.SendRequest(req);
    }

    private void OnLeaveButtonPressed()
    {
        var req = new ClientRequest()
        {
            LeaveRoom = new LeaveRoom()
            {

            }
        };

        RequestSender.SendRequest(req);
    }

    // 事件处理 //

    private bool OnUserJoinRoomEvent(EventSubscription<UserJoinRoomEvent> sub, UserJoinRoomEvent e)
    {
        _room.AddUser(new User(e.User));
        Refresh();

        return false;
    }

    private bool OnUserLeaveRoomEvent(EventSubscription sub, UserLeftRoomEvent e)
    {
        _room.RemoveUser(e.User.Id);
        Refresh();

        return false;
    }

    EventSubscriptions IEventSubscriber.Subscriptions()
    {
        return [
            new EventSubscription<UserJoinRoomEvent>(OnUserJoinRoomEvent),
            new EventSubscription<UserLeftRoomEvent>(OnUserLeaveRoomEvent),
        ];
    }

    // 节点方法 //

    public override void _Ready()
    {
        // 加载场景节点
        _userList = GetNodeOrNull("UserList") as Tree;

        if (GetNodeOrNull("RoomPanel") is Panel roomPanel)
        {
            _roomInfo = roomPanel.GetNodeOrNull("RoomInfo") as Label;
            _startButton = roomPanel.GetNodeOrNull("StartButton") as Button;
            _leaveButton = roomPanel.GetNodeOrNull("LeaveButton") as Button;
        }

        // 初始化用户列表节点
        _userList.SetColumnTitle(0, "用户名");
        _userList.SetColumnTitle(1, "房主");

        // 连接信号
        _startButton.Pressed += OnStartButtonPressed;
        _leaveButton.Pressed += OnLeaveButtonPressed;
    }

    public override void _ExitTree()
    {
        // 取消订阅
        _eventDispatcher.Unsubscribe(this);
    }

}
