
using System;
using System.Collections.Generic;
using System.Linq;
using Chat.Client.V1;
using Chat.Game.V1;
using Godot;

public partial class LocalPlayerController(
    IEventDispatcher eventDispatcher,
    IRequestSender requestSender
) : Node, IPlayerController, IEventSubscriber
{

    public Player Player { get; set; }

    // 事件分发器
    private readonly IEventDispatcher _eventDispatcher = eventDispatcher;

    // 请求发送器
    private readonly IRequestSender _requestSender = requestSender;

    // 事件处理 //

    private bool OnServerTick(EventSubscription<ServerTickEvent> s, ServerTickEvent e)
    {

        // 寻找当前玩家的状态数据
        var state = e.Players.First((player) => player.Id == Player.PlayerID.ToString());

        // 如果没有当前玩家的状态数据
        if (state == null)
        {
            return false;
        }

        // 获取玩家在服务器的位置数据
        var position = new Vector2((float)state.X, (float)state.Y);

        // 如果本地玩家位置和远程玩家位置距离
        // 过远，调整本地玩家位置
        if (Player.Position.DistanceTo(position) > 1.0)
        {
            GD.Print("Fix player position, distance is ", Player.Position.DistanceTo(position));
            Player.Position = position;
        }

        return false;

    }

    EventSubscriptions IEventSubscriber.Subscriptions()
    {
        return
        [
            new EventSubscription<ServerTickEvent>(OnServerTick),
        ];
    }

    // 节点方法 //

    public override void _Ready()
    {
        // 订阅服务器刻事件
        _eventDispatcher?.Subscribe(this);
    }

    public override void _ExitTree()
    {
        // 取消订阅
        _eventDispatcher?.Unsubscribe(this);
    }

    public override void _Process(double delta)
    {

        // 把移动输入发送到服务器
        var direction = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();

        var request = new ClientRequest()
        {
            GameRequest = new ClientGameRequest()
            {
                Move = new Move()
                {
                    X = direction.X,
                    Y = direction.Y,
                }
            }
        };

        _requestSender.SendRequest(request);

        // [FIXME] hard-coded movement speed
        // 移动玩家
        Player.Position += direction * 500.0f * (float)delta;
    }

}
