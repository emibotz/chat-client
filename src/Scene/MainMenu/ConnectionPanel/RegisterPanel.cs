using Godot;
using System;

public partial class RegisterPanel : VBoxContainer
{

    public LineEdit Username { get; private set; }
    public LineEdit Password { get; private set; }
    public LineEdit Password2 { get; private set; }

    public Button RegisterButton { get; private set; }
    public Button GoLoginButton { get; private set; }

    // 节点方法 //

    public override void _Ready()
    {
        // 获取场景节点
        Username = GetNodeOrNull("Username") as LineEdit;
        Password = GetNodeOrNull("Password") as LineEdit;
        Password2 = GetNodeOrNull("Password2") as LineEdit;

        RegisterButton = GetNodeOrNull("LoginOrRegister/RegisterButton") as Button;
        GoLoginButton = GetNodeOrNull("LoginOrRegister/GoLoginButton") as Button;
    }
}
