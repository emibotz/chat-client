
using System;
using System.Collections.Generic;
using Chat.Client.V1;
using Godot;

public partial class RoomListPanel : VBoxContainer, IEventSubscriber
{

    // 网络服务，需要由外部注入
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

    public IRequestSender RequestSender { get; set; }

    // 节点变量 //

    private Tree _roomList;
    private LineEdit _roomName;
    private Button _joinButton;
    private Button _createButton;
    private Button _refreshButton;

    public void Refresh()
    {
        var req = new ClientRequest()
        {
            GetRooms = new GetRooms
            {

            },
        };

        RequestSender.SendRequest(req);
    }

    // 事件处理 //

    private bool OnConnectEvent(EventSubscription<ConnectEvent> sub, ConnectEvent e)
    {
        Refresh();
        return false;
    }

    private bool OnRoomsEvent(EventSubscription<RoomsEvent> sub, RoomsEvent e)
    {
        _roomList.Clear();
        _roomList.CreateItem();

        foreach (var room in e.Rooms)
        {

            var item = _roomList.CreateItem();

            item.SetTextAlignment(0, HorizontalAlignment.Center);
            item.SetText(0, room.Num.ToString().PadLeft(6, '0'));

            item.SetTextAlignment(1, HorizontalAlignment.Left);
            item.SetText(1, room.Name);

            item.SetTextAlignment(2, HorizontalAlignment.Left);
            item.SetText(2, room.Owner);

            item.SetTextAlignment(3, HorizontalAlignment.Center);
            item.SetSuffix(3, " / " + room.MaxUserCount.ToString());
            item.SetText(3, room.UserCount.ToString());

        }

        return true;

    }

    public EventSubscriptions Subscriptions()
    {
        return [
             new EventSubscription<ConnectEvent>(OnConnectEvent),
             new EventSubscription<RoomsEvent>(OnRoomsEvent),
        ];
    }

    // 信号处理 //

    private void OnJoinButtonPressed()
    {

        var selected = _roomList.GetSelected();
        if (selected == null)
        {
            return;
        }

        try
        {
            var num = int.Parse(selected.GetText(0));
            var req = new ClientRequest()
            {
                JoinRoom = new JoinRoom()
                {
                    Num = num,
                },
            };
            RequestSender.SendRequest(req);
        }
        catch (FormatException)
        {
            GD.PrintErr("Room num is not a number.");
        }
        catch (OverflowException)
        {
            GD.PrintErr("Room num cannot fit in a int32.");
        }


    }

    private void OnCreateButtonPressed()
    {

        var req = new ClientRequest()
        {
            CreateRoom = new CreateRoom()
            {
                Name = _roomName.Text,
            },
        };

        RequestSender.SendRequest(req);

    }

    private void OnRefreshButtonPressed()
    {
        // [TODO] 频率限制器？
        Refresh();
    }

    // 节点方法 //

    public override void _Ready()
    {
        // 获取场景节点
        _roomList = GetNodeOrNull("RoomList") as Tree;

        var bottomPanel = GetNodeOrNull("BottomPanel");
        {
            _roomName = bottomPanel.GetNodeOrNull("RoomName") as LineEdit;
            _joinButton = bottomPanel.GetNodeOrNull("JoinButton") as Button;
            _createButton = bottomPanel.GetNodeOrNull("CreateButton") as Button;
            _refreshButton = bottomPanel.GetNodeOrNull("RefreshButton") as Button;
        }

        // 初始化列表节点
        _roomList.SetColumnTitle(0, "房间号");
        _roomList.SetColumnTitle(1, "房间名");
        _roomList.SetColumnTitle(2, "房主");
        _roomList.SetColumnTitle(3, "房间人数");

        // 连接信号
        _joinButton.Pressed += OnJoinButtonPressed;
        _createButton.Pressed += OnCreateButtonPressed;
        _refreshButton.Pressed += OnRefreshButtonPressed;
    }

    public override void _ExitTree()
    {
        // 取消订阅
        _eventDispatcher?.Unsubscribe(this);
    }

}
