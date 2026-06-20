
public interface ISceneManager
{
    public void SwitchScene(Scene scene, params object[] args);
    public void SwitchScene(string name, params object[] args);
}
