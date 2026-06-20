using Godot;
using System;

public partial class LoginPanel : VBoxContainer
{

    public LineEdit Username { get; private set; }
    public LineEdit Password { get; private set; }

    public Button LoginButton { get; private set; }
    public Button GoRegisterButton { get; private set; }

    // 节点方法 //

    public override void _Ready()
    {
        // 获取节点
        Username = GetNodeOrNull("Username") as LineEdit;
        Password = GetNodeOrNull("Password") as LineEdit;

        LoginButton = GetNodeOrNull("LoginOrRegister/LoginButton") as Button;
        GoRegisterButton = GetNodeOrNull("LoginOrRegister/GoRegisterButton") as Button;
    }

}
