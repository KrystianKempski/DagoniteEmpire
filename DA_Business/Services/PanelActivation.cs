using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Services
{
    public class PanelActivation
    {
        private bool _leftPanelActive;
        public event Action OnChange;
        public bool LeftPanelActive
        {
            get { return _leftPanelActive; }
            set
            {
                if (_leftPanelActive != value)
                {
                    _leftPanelActive = value;
                    LeftPanelStateChanged();
                }
            }
        }

        private void LeftPanelStateChanged() => OnChange?.Invoke();
    }
}
