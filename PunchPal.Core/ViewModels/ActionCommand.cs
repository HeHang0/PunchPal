using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class ActionCommand: ICommand
    {
        public event EventHandler CanExecuteChanged;
        private Action _action;
        private Func<bool> _canExecute;
        public ActionCommand(Action action, Func<bool> canExecute=null)
        {
            _action = action;
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }
        public void Execute(object parameter)
        {
            _action();
        }
    }
}
