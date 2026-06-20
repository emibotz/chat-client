using Godot;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Http = System.Net.Http;

public partial class ConnectionPanel : VBoxContainer
{

    // 信号定义 //

    [Signal]
    public delegate void ConnectedEventHandler(string address, string token);

    // 场景节点 //

    private LineEdit _address;
    private LoginPanel _loginPanel;
    private RegisterPanel _registerPanel;

    // 网络相关变量 //

    private Http.HttpClient _client;

    // 信号方法 //

    private void OnGoLoginButtonPressed()
    {
        _registerPanel.Hide();
        _loginPanel.Show();

        if (_loginPanel.Username.Text == "" && _loginPanel.Password.Text == "")
        {
            _loginPanel.Username.Text = _registerPanel.Username.Text;
            _loginPanel.Password.Text = _registerPanel.Password.Text;
        }
    }

    private void OnGoRegisterButtonPressed()
    {
        _registerPanel.Show();
        _loginPanel.Hide();

        if (_registerPanel.Username.Text == "" && _registerPanel.Password.Text == "")
        {
            _registerPanel.Username.Text = _loginPanel.Username.Text;
            _registerPanel.Password.Text = _loginPanel.Password.Text;
        }
    }

    private void OnLoginButtonPressed()
    {

        // [TODO]
        // 这里应该做登录参数检测。

        var address = _address.Text;
        var username = _loginPanel.Username.Text;
        var password = _loginPanel.Password.Text;

        var request = new LoginRequest()
        {
            Username = username,
            Password = password
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new Http.StringContent(json, Encoding.UTF8, "application/json");

        var response = _client.PostAsync("http://" + address + "/api/user/login", content).GetAwaiter().GetResult();

        if (response.StatusCode != HttpStatusCode.OK)
        {
            GD.PrintErr("Connection Failed! Status Code: ", response.StatusCode);
            return;
        }

        var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        GD.Print("Received response: ", responseContent);

        var resp = JsonConvert.DeserializeObject<ServerResponse<LoginResponse>>(responseContent);
        if (resp == null)
        {
            GD.PrintErr("Failed to deserialize response. Raw data: ", responseContent);
            return;
        }

        EmitSignal(SignalName.Connected, address, resp.Data.Token);

    }

    private void OnRegisterButtonPressed()
    {

        // [TODO]
        // 这里应该做登录参数检测。

        var address = _address.Text;
        var username = _registerPanel.Username.Text;
        var password = _registerPanel.Password.Text;
        var password2 = _registerPanel.Password2.Text;

        if (password != password2)
        {
            GD.PrintErr("password does not match!");
            return;
        }

        var request = new RegisterRequest()
        {
            Username = username,
            Password = password
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new Http.StringContent(json, Encoding.UTF8, "application/json");

        var response = _client.PostAsync("http://" + address + "/api/user/register", content).GetAwaiter().GetResult();
        var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (response.StatusCode != HttpStatusCode.Created)
        {
            GD.PrintErr("Connection Failed! Status Code: ", response.StatusCode, " Response: ", responseContent);
            return;
        }
        var resp = JsonConvert.DeserializeObject<ServerResponse<RegisterResponse>>(responseContent);
        if (resp == null)
        {
            GD.PrintErr("Failed to deserialize response. Raw data: ", responseContent);
            return;
        }

        EmitSignal(SignalName.Connected, address, resp.Data.Token);

    }

    // 节点方法 //

    public override void _Ready()
    {

        // 获取子节点
        _address = GetNodeOrNull("Address") as LineEdit;
        _loginPanel = GetNodeOrNull("LoginPanel") as LoginPanel;
        _registerPanel = GetNodeOrNull("RegisterPanel") as RegisterPanel;

        // 连接节点信号
        _loginPanel.LoginButton.Pressed += OnLoginButtonPressed;
        _loginPanel.GoRegisterButton.Pressed += OnGoRegisterButtonPressed;

        _registerPanel.RegisterButton.Pressed += OnRegisterButtonPressed;
        _registerPanel.GoLoginButton.Pressed += OnGoLoginButtonPressed;

        // 创建 HTTP 客户端
        _client = new Http.HttpClient();

    }
}
