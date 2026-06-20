using Godot;
using System;

public partial class Game : Scene, IEventSubscriber
{

    // 场景节点 //

    private RoomMenu _roomMenu;
    private GameMachine _gameMachine;

    // 场景处理 //

    public Room Room { get; private set; }
    private ISceneManager _sceneManager;
    private IRequestSender _requestSender;
    private IEventDispatcher _eventDispatcher;

    public override void SwitchFrom(Scene scene, params object[] args)
    {
        // 从参数中获取房间
        foreach (var arg in args)
        {
            if (arg is Room room)
            {
                Room = room;
            }
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
        }

        // 订阅事件
        _eventDispatcher?.Subscribe(this);

        // 为子节点注入变量
        _roomMenu.Room = Room;
        _roomMenu.RequestSender = _requestSender;
        _roomMenu.EventDispatcher = _eventDispatcher;

        _gameMachine.RequestSender = _requestSender;
        _gameMachine.EventDispatcher = _eventDispatcher;

        // 刷新界面
        _roomMenu.Refresh();
    }

    // 事件处理 //

    private bool OnRoomLeftEvent(EventSubscription sub, RoomLeftEvent e)
    {
        _sceneManager.SwitchScene("MainMenu", "online");
        return false;
    }

    private bool OnGameStopEvent(EventSubscription sub, GameStopEvent e)
    {
        _roomMenu.Show();
        _gameMachine.Hide();
        return false;
    }

    private bool OnGameStartEvent(EventSubscription sub, GameStartEvent e)
    {
        _roomMenu.Hide();
        _gameMachine.Show();
        return false;
    }

    private bool OnUserLeftRoomEvent(EventSubscription sub, UserLeftRoomEvent e)
    {
        if (e.User.Id == Room.Owner.Id)
        {
            _sceneManager.SwitchScene("MainMenu", "online");
        }

        return false;
    }

    EventSubscriptions IEventSubscriber.Subscriptions()
    {
        return [
            new EventSubscription<RoomLeftEvent>(OnRoomLeftEvent),
            new EventSubscription<GameStopEvent>(OnGameStopEvent),
            new EventSubscription<GameStartEvent>(OnGameStartEvent),
            new EventSubscription<UserLeftRoomEvent>(OnUserLeftRoomEvent)
        ];
    }

    // 节点方法 //

    public override void _Ready()
    {
        // 获取场景节点
        _roomMenu = GetNodeOrNull("RoomMenu") as RoomMenu;
        _gameMachine = GetNodeOrNull("GameMachine") as GameMachine;
    }

    public override void _ExitTree()
    {
        // 取消订阅
        _eventDispatcher?.Unsubscribe(this);
    }

}
