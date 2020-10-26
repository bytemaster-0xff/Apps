using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.ManCaveController.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.ManCaveController.ViewModels
{
    public class BlindsViewModel : ViewModelBase
    {

        public async void BlindAction(Object param)
        {
            var commandParameters = param.ToString().Split('.');
            
            var blindIndex = Convert.ToInt32(commandParameters[0]);
            var blindCommand = commandParameters[1];

            await BlindController.Instance.ChangeBlindState(blindIndex, blindCommand);
        }

        RelayCommand _blindActionCommand;
        public RelayCommand BlindActionCommand
        {
            get
            {
                if (_blindActionCommand == null)
                    _blindActionCommand = new RelayCommand((parm) => BlindAction(parm));

                return _blindActionCommand;
            }
        }
    }
}
