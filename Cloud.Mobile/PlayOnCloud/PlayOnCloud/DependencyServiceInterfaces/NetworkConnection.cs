//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.Content;
//using Android.Net;
//using PlayOnCloud.DependencyServiceInterfaces;
//using Xamarin.Forms;
//using Application = Android.App.Application;

//[assembly: Dependency(typeof(NetworkConnection))]
//namespace PlayOnCloud.DependencyServiceInterfaces
//{
//   public class NetworkConnection:INetworkConnection
//    {
//        public bool IsConnected { get; set; }
//        public void CheckNetworkConnection()
//        {
//            //var connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
//            //var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
//            //if (activeNetworkInfo != null && activeNetworkInfo.IsConnectedOrConnecting)
//            //{
//            //    IsConnected = true;
//            //}
//            //else
//            //{
//            //    IsConnected = false;
//            //}
//        }
//    }
//}
