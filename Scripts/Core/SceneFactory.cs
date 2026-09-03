using Godot;

public class SceneFactory
{
    public static Control CreateScene(string scenePath)
    {
        var packed = GD.Load<PackedScene>(scenePath);
        return packed?.Instantiate<Control>();
    }
}