namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.ViewModels
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// A generic implementation of <see cref="ICommand"/>, getting the action to perform in the constructor
    /// </summary>
    public class CommandHandler : ICommand
    {
        private readonly Action<object> action;
        private bool canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="action">The action to run when the command is invoked.</param>
        public CommandHandler(Action action)
        {
            this.action = parameter => action();
            this.canExecute = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="action">The action to run when the command is invoked.</param>
        public CommandHandler(Action<object> action)
        {
            this.action = action;
            this.canExecute = true;
        }

        /// <summary>
        /// Occurs when the action's <see cref="CanExecute"/> changes.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        #region Implementation of ICommand

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        public bool CanExecute(object parameter)
        {
            return this.canExecute;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            this.action(parameter);
        }
        
        #endregion

        /// <summary>
        /// Updates the value of whether the command can execute (cannot define this in a property since the name is already taken 
        /// by the <see cref="ICommand"/> interface.
        /// </summary>
        /// <param name="updatedCanExecute">The new value</param>
        public void UpdateCanExecute(bool updatedCanExecute)
        {
            this.canExecute = updatedCanExecute;
            EventHandler canExecuteChanged = this.CanExecuteChanged;
            canExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
