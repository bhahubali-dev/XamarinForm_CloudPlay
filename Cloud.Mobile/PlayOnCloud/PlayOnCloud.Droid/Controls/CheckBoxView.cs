using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
namespace PlayOnCloud.Droid.Controls
{
    [Register("CheckBoxView")]
    public class CheckBoxView : CheckBox
    {
        public CheckBoxView()
        {
            Initialize();
        }

        //public CheckBoxView(CGRect bounds)
        //    : base(bounds)
        //{
        //    Initialize();
        //}

        //public bool Checked
        //{
        //    set { Selected = value; }
        //    get { return Selected; }
        //}

        void Initialize()
        {
           // TouchUpInside += (sender, args) => Selected = !Selected;
        }
    }
}