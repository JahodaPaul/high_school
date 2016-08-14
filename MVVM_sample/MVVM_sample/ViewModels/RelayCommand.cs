using System;
using System.Windows.Input;

namespace MVVM_test1.ViewModels
{
        public class RelayCommand : ICommand
        {
            // Inbuilt delegate. Performs an action given the arguments. Very general purpose.
            private readonly Action _action;
            
            
            public RelayCommand(Action action)
            {
                _action = action;
            }

            /*This is the method which does actual work which is intended for the Command. This method execute only 
             if CanExecute method returns true. It takes an object as an argument and we generally pass
             a delegate into this method. The delegate holds a method reference which is supposed to execute when Command is fired. */
            public void Execute(object parameter)
            {
                _action();
            }

            /*CanExecute method takes an object as an argument and return bool. If it returns true, associated command can be executed 
            Often we pass a delegate to this method as an object which returns a bool. 
            For that we can use inbuilt delegate like Action, Predicate or Func.*/
            public bool CanExecute(object parameter)
            {
                return true;
            }

            //pragma warnings lines below get rid of warning: CanExecuteChanged never used;
            #pragma warning disable 67
            public event EventHandler CanExecuteChanged;
            #pragma warning restore 67
        }
}
//Pavel Jahoda