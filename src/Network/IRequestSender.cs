
using Chat.Client.V1;

public static class ApiVersion
{
    public static string Prefix = "dev-";
    public static string Suffix = "";

    public static int MajorVersion = 0;
    public static int MinorVersion = 0;
    public static int PatchVersion = 1;

    public static string Version = string.Format("{0}{1}.{2}.{3}{4}", Prefix, MajorVersion, MinorVersion, PatchVersion, Suffix);
}

public interface IRequestSender
{
    public void SendRequest(ClientRequest request);
}
