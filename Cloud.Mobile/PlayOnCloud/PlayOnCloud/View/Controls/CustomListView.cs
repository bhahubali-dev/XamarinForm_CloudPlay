using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public class CustomListView : ListView
    {
        public static readonly BindableProperty CanEditRowProperty =
            BindableProperty.Create("CanEditRow", typeof(bool), typeof(CustomListView), false);

        public static readonly BindableProperty MoveRowCommandProperty =
            BindableProperty.Create("MoveRowCommand", typeof(ICommand), typeof(CustomListView), null);

        public CustomListView()
        {
            HasUnevenRows = true;           
        }

        public bool CanEditRow
        {
            get { return (bool) GetValue(CanEditRowProperty); }
            set { SetValue(CanEditRowProperty, value); }
        }

        public ICommand MoveRowCommand
        {
            get { return (ICommand) GetValue(MoveRowCommandProperty); }
            set { SetValue(MoveRowCommandProperty, value); }
        }


       

        public void MoveRow(int sourceIndex, int destinationIndex)
        {
            if (MoveRowCommand != null)
                MoveRowCommand.Execute(new List<int> {sourceIndex, destinationIndex});
        }
    }
}