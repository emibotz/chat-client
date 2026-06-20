using Godot;
using System;

public partial class Main : Node, ISceneManager
{

    // 场景处理 //
    private Scene _scene;
    public Scene CurrentScene
    {
        get => _scene;
    }

    public void SwitchScene(Scene newScene, params object[] args)
    {
        args = [this, _networkService, .. args];

        var old = _scene;
        var newArgs = old?.SwitchTo(newScene, args);
        old?.QueueFree();

        if (newScene == null)
        {
            return;
        }

        if (args != null && newArgs != null)
        {
            args = [.. args, .. newArgs];
        }

        _scene = newScene;
        AddChild(_scene);
        _scene.SwitchFrom(old, args);
    }

    public void SwitchScene(string name, params object[] args)
    {
        var packedScene = GD.Load(string.Format("res://scene/{0}/{0}.tscn", name)) as PackedScene;
        if (packedScene.Instantiate() is Scene scene)
        {
            SwitchScene(scene, args);
        }
    }

    // 网络服务
    private NetworkService _networkService;

    // 信号处理 //

    // [FIXME] 可能移动到其他类中更好。
    private void OnConnectionPanelConnected(string address, string token)
    {
        _ = _networkService.ConnectAsync(new Uri("ws://" + address + "/ws"), token);
    }

    // 节点方法 //

    public override void _Ready()
    {
        // 创建网络服务，并将其添加到场景树中。
        _networkService = new();
        AddChild(_networkService);

        // 读取主菜单
        SwitchScene("MainMenu");
        if (_scene is MainMenu mainMenu)
        {
            // 连接信号
            mainMenu.ConnectionPanel.Connected += OnConnectionPanelConnected;
        }

        // 调试

        _networkService.Subscribe(new EventSubscription<ErrorEvent>((sub, e) =>
        {
            GD.Print("Received error event, message: ", e.Error);
            return false;
        }));

        GD.Print("API VERSION: ", ApiVersion.Version);
    }

}
