using System;
using System.Linq;
using System.Windows.Input;

namespace Calculator.Commands
{
    class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action execute;
        private Func<bool> canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            if (this.canExecute == null)
            {
                return true;
            }
            return this.canExecute();
        }

        public void Execute(object parameter)
        {
            this.execute();
        }
    }
}
