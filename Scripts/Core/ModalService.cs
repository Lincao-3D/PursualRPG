using Godot;
using System;

namespace PursualRPG.Scripts.Core;

public partial class ModalService : CanvasLayer
{
    public static ModalService Instance { get; private set; }
    private Control _modalContainer;
    private Label _promptLabel;
    private LineEdit _inputField;
    private Button _submitButton;
    private Action<string> _currentCallback;

    public override void _Ready()
    {
        Instance = this;
        Layer = 100;
        
        _modalContainer = new Control { Visible = false };
        _modalContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_modalContainer);

        var bg = new ColorRect { Color = new Color(0, 0, 0, 0.7f) };
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _modalContainer.AddChild(bg);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.Center);
        vbox.CustomMinimumSize = new Vector2(400, 200);
        _modalContainer.AddChild(vbox);

        _promptLabel = new Label { Text = "Enter value:" };
        vbox.AddChild(_promptLabel);

        _inputField = new LineEdit();
        vbox.AddChild(_inputField);

        _submitButton = new Button { Text = "Submit" };
        _submitButton.Pressed += OnSubmitPressed;
        vbox.AddChild(_submitButton);
    }

    public void ShowPrompt(string promptText, Action<string> callback)
    {
        _promptLabel.Text = promptText;
        _inputField.Clear();
        _currentCallback = callback;
        _modalContainer.Visible = true;
        _inputField.GrabFocus();
    }

    private void OnSubmitPressed()
    {
        var text = _inputField.Text;
        _modalContainer.Visible = false;
        _currentCallback?.Invoke(text);
        _currentCallback = null;
    }
}