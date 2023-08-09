using MyJavaTest.WpfClient.Infrastructure.Commands.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyJavaTest.WpfClient.Infrastructure.Commands
{
    public class LambdaCommand : Command
    {
        private readonly Action<object> _Execute;
        private readonly Func<object, bool> _CanExecute;

        public LambdaCommand(Action Execute, Func<bool> CanExecute = null)
            : this(
                Execute: p => Execute(),
                CanExecute: CanExecute is null ? (Func<object, bool>)null : p => CanExecute())
        {

        }

        public LambdaCommand(Action<object> Execute, Func<object, bool> CanExecute = null)
        {
            _Execute = Execute ?? throw new ArgumentNullException(nameof(Execute));
            _CanExecute = CanExecute;
        }

        protected override bool CanExecute(object parameter) => _CanExecute?.Invoke(parameter) ?? true;

        protected override void Execute(object parameter) => _Execute(parameter);
    }
}
