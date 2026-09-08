using Godot;
using System;
using System.Threading.Tasks;

namespace PursualRPG.Scripts.UI
{
    public static class TweenManager
    {
        public static async Task SlideInHoldOut(Control node, Vector2 hiddenPos, Vector2 shownPos, float holdSeconds, Action onDone = null)
        {
            if (node == null) return;
            node.Position = hiddenPos;

            var tweenIn = node.CreateTween();
            tweenIn.TweenProperty(node, "position", shownPos, 0.5f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
            await AwaitTween(tweenIn);

            await node.ToSignal(node.GetTree().CreateTimer(holdSeconds), SceneTreeTimer.SignalName.Timeout);

            var tweenOut = node.CreateTween();
            tweenOut.TweenProperty(node, "position", hiddenPos, 0.5f).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
            await AwaitTween(tweenOut);

            onDone?.Invoke();
        }

        public static Tween PulseModulate(CanvasItem node, Color a, Color b, float periodSeconds)
        {
            if (node == null) return null;
            var tween = node.CreateTween().SetLoops();
            tween.TweenProperty(node, "modulate", b, periodSeconds / 2f);
            tween.TweenProperty(node, "modulate", a, periodSeconds / 2f);
            return tween;
        }

        public static async Task AwaitTween(Tween tween)
        {
            if (tween != null && tween.IsValid())
            {
                await tween.ToSignal(tween, Tween.SignalName.Finished);
            }
        }
    }
}