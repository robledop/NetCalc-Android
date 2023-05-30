using Android.App;
using Android.Content.PM;
using Cirrious.MvvmCross.Droid.Views;

namespace NetCalc.Droid
{
    [Activity(
        Label = "NetCalc"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , NoHistory = true)]
    public class SplashScreen : MvxSplashScreenActivity
    {
        public SplashScreen()
            : base(Resource.Layout.SplashScreen)
        {
        }
    }
}