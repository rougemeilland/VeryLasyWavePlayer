using System.Windows.Input;

namespace WavePlayer.GUI
{
    // See https://stackoverflow.com/questions/2271342/mousebinding-the-mousewheel-to-zoom-in-wpf-and-mvvm
    public class MouseWheelGesture
        : MouseGesture
    {
        public MouseWheelGesture()
            : base(MouseAction.WheelClick)
        {
        }

        public MouseWheelGesture(ModifierKeys modifiers)
            : base(MouseAction.WheelClick, modifiers)
        {
        }

        public static MouseWheelGesture CtrlDown
            => new MouseWheelGesture(ModifierKeys.Control) { Direction = MouseWheelDirection.Down };

        public static MouseWheelGesture CtrlUp
            => new MouseWheelGesture(ModifierKeys.Control) { Direction = MouseWheelDirection.Up };

        public MouseWheelDirection Direction { get; set; }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (!base.Matches(targetElement, inputEventArgs))
                return false;
            if (!(inputEventArgs is MouseWheelEventArgs args))
                return false;
            switch (Direction)
            {
                case MouseWheelDirection.None:
                    return args.Delta == 0;
                case MouseWheelDirection.Up:
                    return args.Delta > 0;
                case MouseWheelDirection.Down:
                    return args.Delta < 0;
                default:
                    return false;
            }
        }
    }
}
