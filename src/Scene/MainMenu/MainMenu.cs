using Godot;
using System;

public partial class MainMenu : Scene, IEventSubscriber
{

    public override void SwitchFrom(Scene scene, params object[] args)
    {
        foreach (var arg in args)
        {
            if (arg is ISceneManager sceneManager)
            {
                _sceneManager = sceneManager;
            }
            if (arg is IRequestSender requestSender)
            {
                _requestSender = requestSender;
            }
            if (arg is IEventDispatcher eventDispatcher)
            {
                _eventDispatcher = eventDispatcher;
            }
            if (arg is string s)
            {
                switch (s)
                {
                    case "online":
                        {
                            ConnectionPanel.Hide();
                            RoomListPanel.Show();
                            RoomListPanel.CallDeferred(RoomListPanel.MethodName.Refresh);
                            break;
                        }
                }
            }
        }

        _eventDispatcher?.Subscribe(this);

        RoomListPanel.RequestSender = _requestSender;
        RoomListPanel.EventDispatcher = _eventDispatcher;
    }

    private ISceneManager _sceneManager;
    private IRequestSender _requestSender;
    private IEventDispatcher _eventDispatcher;

    // 场景节点 //

    public ConnectionPanel ConnectionPanel { get; private set; }
    public RoomListPanel RoomListPanel { get; private set; }

    // 信号处理 //

    private void OnConnectionPanelConnected(string address, string token)
    {
        ConnectionPanel.Hide();
        RoomListPanel.Show();
    }

    // 事件处理 //

    private bool OnRoomJoinedEvent(EventSubscription sub, RoomJoinedEvent e)
    {
        var room = new Room(e.Num, e.Name, e.Owner, e.Users);
        _sceneManager.SwitchScene("Game", room);

        return false;
    }

    EventSubscriptions IEventSubscriber.Subscriptions()
    {
        return [
            new EventSubscription<RoomJoinedEvent>(OnRoomJoinedEvent),
        ];
    }

    // 节点方法 //

    public override void _Ready()
    {
        // 读取场景节点
        ConnectionPanel = GetNodeOrNull("ConnectionPanel") as ConnectionPanel;
        RoomListPanel = GetNodeOrNull("RoomListPanel") as RoomListPanel;

        // 连接信号
        ConnectionPanel.Connected += OnConnectionPanelConnected;
    }

    public override void _ExitTree()
    {
        _eventDispatcher?.Unsubscribe(this);
    }

}
