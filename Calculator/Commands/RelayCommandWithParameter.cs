using System;
using System.Linq;
using System.Windows.Input;

namespace Calculator.Commands
{
    class RelayCommandWithParameter : ICommand
    {
        private readonly Action<string> execute;
        private readonly Predicate<string> canExecute;

        public RelayCommandWithParameter(Action<string> execute, Predicate<string> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (this.canExecute == null)
            {
                return true;
            }
            return this.canExecute(parameter.ToString());
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            this.execute(parameter.ToString());
        }
    }
}
