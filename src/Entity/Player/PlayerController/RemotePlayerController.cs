
using System.Linq;
using Godot;

public partial class RemotePlayerController(
    IEventDispatcher eventDispatcher
) : PlayerController, IEventSubscriber
{

    private float _progress;
    private float _targetProgress;
    private Vector2 _lastPosition;
    private Vector2 _targetPosition;

    // 事件分发器
    private readonly IEventDispatcher _eventDispatcher = eventDispatcher;

    // 事件处理 //

    private bool OnServerTick(EventSubscription s, ServerTickEvent e)
    {
        // 寻找当前玩家的数据
        var state = e.Players.FirstOrDefault((player) => player.Id == Player.PlayerId, null);

        // 如果没有当前玩家的数据，应该删除这个玩家！
        if (state == null)
        {
            return false;
        }

        // 更新玩家位置
        _progress = 0.0f;
        _targetProgress = (float)(1.0 / 60.0);
        _lastPosition = Player.Position;
        _targetPosition = new Vector2((float)state.X, (float)state.Y);

        return false;
    }

    EventSubscriptions IEventSubscriber.Subscriptions()
    {
        return [
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
        if (_targetProgress == 0.0f)
        {
            return;
        }

        _progress += (float)delta;
        if (_progress > _targetProgress)
        {
            _progress = _targetProgress;
        }

        Player.Position = _lastPosition.Lerp(_targetPosition, _progress / _targetProgress);
    }
}
