using Godot;

namespace PursualRPG.Scripts.Core;

public static class SceneFactory
{
    public static Control CreateScene(string scenePath)
    {
        var packed = GD.Load<PackedScene>(scenePath);
        return packed?.Instantiate<Control>();
    }
}