using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Services
{
    public class CallbackService
    {
        public int SelectedCharId { get; private set; }

        public void SetSelectedChar(int Id)
        {
            if (SelectedCharId != Id)
            {
                SelectedCharId = Id;
                NotifyStateChanged();
            }
        }

        public event Action OnChange; // event raised when changed

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
