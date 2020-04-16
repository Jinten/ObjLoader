using Livet.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewTest.Utilities
{
    public class ViewModelCommandHandler
    {
        public ViewModelCommand Get(Action execute, Func<bool> canExecute = null)
        {
            if(_Command == null)
            {
                _Command = new ViewModelCommand(execute, canExecute);
            }

            return _Command;
        }

        ViewModelCommand _Command = null;
    }
}
