using Godot;
using System;

public partial class Player : Node2D
{

    // 节点变量 //

    private Sprite2D _sprite = null;
    private Label _nameLabel = null;

    // 玩家属性 //

    public Guid PlayerId { get; set; }
    public Guid UserID { get; set; }

    private string _name;
    public string PlayerName
    {
        get => _name;
        set
        {
            // 如果有名字标签，将标签文本设置为玩家名
            if (_nameLabel != null)
            {
                _nameLabel.Text = value;
            }
            _name = value;
        }
    }

    // 节点方法 //

    public override void _Ready()
    {
        // 读取子节点
        _sprite = GetNodeOrNull("Sprite2D") as Sprite2D;
        _nameLabel = GetNodeOrNull("Label") as Label;
    }

}
