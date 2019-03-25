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
using PlayOnCloud;
using PlayOnCloud.Droid.Renderers;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(NavigationFrame), typeof(NavigationFrameRenderer))]
namespace PlayOnCloud.Droid.Renderers
{
    public class NavigationFrameRenderer : RoundFrameRenderer
    {
    }
}