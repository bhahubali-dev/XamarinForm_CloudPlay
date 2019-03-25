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

namespace PlayOnCloud.Droid.Tools
{
    internal static class InputTools
    {
        internal static ViewGroup FindFirstResponder(ViewGroup view)
        {
            //if (view.IsFirstResponder)
            //    return view;

            //foreach (var subview in view.Subviews)
            //{
            //    var responder = FindFirstResponder(subview);
            //    if (responder != null)
            //        return responder;
            //}

            return null;
        }
    }
}