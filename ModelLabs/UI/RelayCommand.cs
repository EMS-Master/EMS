using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UI
{
    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action<object> execute)
      : base(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        : base(execute, canExecute)
        {
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private Action<T> execute;
        private Predicate<T> canExecute;



        public RelayCommand(Action<T> execute)
       : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public event EventHandler IsActiveChanged;


        public void SetCanExecute(Predicate<T> method)
        {
            canExecute = method;
        }

        public void SetExecute(Action<T> method)
        {
            execute = method;
        }


        public bool CanExecute(object parameter)
        {
            return this.canExecute == null ? true : this.canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            this.execute((T)parameter);
        }
    }
}
