using Godot;

public partial class VideoBgPlayer : VideoStreamPlayer
{
    public override void _Ready()
    {   
        Finished += () => Play();
    }
}
