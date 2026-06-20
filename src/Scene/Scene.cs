
using Godot;

public abstract partial class Scene : Node
{

    public virtual object[] SwitchTo(Scene scene, params object[] args)
    {
        return null;
    }

    public virtual void SwitchFrom(Scene scene, params object[] args) { }

}
