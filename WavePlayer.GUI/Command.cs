using System;
using System.Windows.Input;

namespace WavePlayer.GUI
{
    public class Command
        : ICommand
    {
        private readonly Func<object, bool> _canExecuteHandler;
        private readonly Action<object> _executeHandler;

        public Command(Action<object> executeHandler)
            : this(p => true, executeHandler)
        {
        }

        public Command(Func<object, bool> canExecuteHandler, Action<object> executeHandler)
        {
            _canExecuteHandler = canExecuteHandler;
            _executeHandler = executeHandler;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
            => _canExecuteHandler(parameter);

        public void Execute(object parameter) => _executeHandler(parameter);

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                try
                {
                    CanExecuteChanged(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
