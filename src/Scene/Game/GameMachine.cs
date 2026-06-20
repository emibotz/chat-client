using Godot;
using System;
using System.Collections.Generic;

public partial class GameMachine : Node2D, IEventSubscriber
{

    private IRequestSender _requestSender;
    public IRequestSender RequestSender
    {
        set => _requestSender = value;
    }

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

    // 玩家处理 //

    private PackedScene _playerScene;

    private Guid _playerId;
    private readonly Dictionary<Guid, PlayerController> _playerControllers = [];

    // 事件处理 //

    private bool OnGameStartEvent(EventSubscription sub, GameStartEvent e)
    {
        _playerId = e.PlayerId;
        return false;
    }

    private bool OnServerTickEvent(EventSubscription sub, ServerTickEvent e)
    {

        // 清理处理器
        foreach (var c in _playerControllers)
        {
            if (!c.Value.IsInsideTree())
            {
                _playerControllers.Remove(c.Key);
            }
        }

        // 处理玩家
        foreach (var p in e.Players)
        {
            if (!_playerControllers.ContainsKey(p.Id))
            {
                PlayerController controller;

                if (p.Id == _playerId)
                {
                    controller = new LocalPlayerController(_eventDispatcher, _requestSender);
                }
                else
                {
                    controller = new RemotePlayerController(_eventDispatcher);
                }

                var player = _playerScene.Instantiate() as Player;

                controller.Player = player;
                AddChild(controller);

                AddChild(player);

                player.PlayerId = p.Id;
                player.PlayerName = p.Name;

                _playerControllers[p.Id] = controller;
            }
        }

        // [TODO] 处理消息

        return false;
    }

    EventSubscriptions IEventSubscriber.Subscriptions()
    {
        return [
            new EventSubscription<GameStartEvent>(OnGameStartEvent),
            new EventSubscription<ServerTickEvent>(OnServerTickEvent),
        ];
    }

    // 节点方法 //

    public override void _Ready()
    {
        // 读取玩家场景
        _playerScene = GD.Load("res://entity/Player/Player.tscn") as PackedScene;
    }

    public override void _ExitTree()
    {
        // 取消订阅
        _eventDispatcher.Unsubscribe(this);
    }


}
