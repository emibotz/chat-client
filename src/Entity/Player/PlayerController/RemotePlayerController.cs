
using System.Linq;
using Godot;

public partial class RemotePlayerController(
    IEventDispatcher eventDispatcher
) : Node, IPlayerController, IEventSubscriber
{

    public Player Player { get; set; }

    private Tween _tween = null;

    // 事件分发器
    private readonly IEventDispatcher _eventDispatcher = eventDispatcher;

    // 事件处理 //

    private void UpdatePosition(Vector2 position)
    {

        if (_tween == null)
        {
            return;
        }

        _tween.Stop();
        _tween.TweenProperty(Player, Node2D.PropertyName.Position.ToString(), position, 1.0 / 60.0);
    }

    private bool OnServerTick(EventSubscription s, ServerTickEvent e)
    {
        // 寻找当前玩家的数据
        var state = e.Players.First((player) => player.Id == Player.PlayerID.ToString());

        // [TODO]
        // 如果没有当前玩家的数据，应该删除这个玩家！
        if (state == null)
        {
            return false;
        }

        // 更新玩家位置
        var position = new Vector2((float)state.X, (float)state.Y);
        CallDeferred(MethodName.UpdatePosition, position);

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

        // 创建 Tween 对象
        _tween = CreateTween();
    }

    public override void _ExitTree()
    {
        // 取消订阅
        _eventDispatcher?.Unsubscribe(this);
    }

    public override void _Process(double delta)
    {
        // [TODO]
    }
}
