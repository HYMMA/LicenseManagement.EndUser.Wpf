using System;
using System.Windows.Input;

namespace Hymma.Lm.EndUser.Wpf.Commands
{
    internal class RelayCommand : ICommand
    {
        private Action<object> executeAction;
        private Predicate<object> canExecutePredicate;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this.executeAction = execute;
            this.canExecutePredicate = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return canExecutePredicate(parameter);
        }

        public void Execute(object parameter)
        {
            executeAction(parameter);
        }
    }
}
