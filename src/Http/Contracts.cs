
using Newtonsoft.Json;

public class LoginRequest
{
    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }
}

public class RegisterRequest
{
    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }
}

public class ServerResponse<T>
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("data")]
    public T Data { get; set; }
}

public class LoginResponse
{
    [JsonProperty("token")]
    public string Token { get; set; }
}

public class RegisterResponse
{
    [JsonProperty("token")]
    public string Token { get; set; }
}
