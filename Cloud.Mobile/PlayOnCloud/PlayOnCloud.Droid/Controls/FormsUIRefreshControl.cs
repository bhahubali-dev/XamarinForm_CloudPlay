using System.Windows.Input;

namespace PlayOnCloud.Droid.Controls
{
    public class FormsUIRefreshControl
    {
        public ICommand RefreshCommand { get; set; }

        public bool IsRefreshing { get; set; }
    }
}