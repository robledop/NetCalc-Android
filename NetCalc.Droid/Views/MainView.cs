using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using NetCalc.Core.Models;
using NetCalc.Core.ViewModels;
using NetCalc.Droid.Common;
using NetCalc.Droid.MvxSherlockActionBar;
using Android.Gms.Ads;
using Xamarin.InAppBilling;
using Xamarin.InAppBilling.Utilities;

namespace NetCalc.Droid.Views
{
	[Activity(Label = "NetCalc", Theme = "@style/Theme.AppCompat.Light")]
	public class MainView : MvxSherlockFragmentActivity
	{
		private const string PRODUCT_ID_REMOVE_ADS = "netcalc_remove_ads";

		private readonly string _publicKey = Security.Unify(
												 new[] { "qTP7lebB8fYm/rqDdRLMRQEaBnsAfuoTHtqgFmtRfhzNabSiUtQwS",
				"YxEkgdVM6I8vFDkH8FZ/+z0JOZoUaUNxz1GVHdYPBXEA830xG+WWM3",
				"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKcuzqAj4sAHTmeh",
				"Ffr/-=a+0JwrNxuvNnz7aFlEViHuMZrYSlVOt3hmIZ5rU/sOSC5ss",
				"hwihiMvjgIhl3/PIgPfVV9yc5AThvb1K9BliW8dKqaQL7sy58/nXbfjSt7o7GOhIAy+k7QIzdr-a",
				"Vjhc+wI5D9cftwOtPlVspudaLFxRkKA3pJvZNKB72sUdTe6apvtku",
				"may7W3alRCBUaBk8H0+U109mybeQIbCHNk2jYSHBOdsIgxn/g2"
			},
												 new[] { 4, 5, 0, 2, 6, 1, 3 },
												 new[] { "zdr-a", "DAQAB", "apvtku", "/PuPtk", "cuzq", "CAQE", "-=a+", "LUU2" });

		private InAppBillingServiceConnection _serviceConnection;

		private IList<Product> _products;

		private MvxSpinner _subnetMaskDropDown;
		private AdView _ad;
		private LinearLayout _adLayout;

		protected override void OnCreate(Bundle bundle)
		{
			//SetTheme(Resource.Style.Theme_Sherlock_Light);
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.MainView);


			// The Action Bar is a window feature. The feature must be requested
			// before setting a content view. Normally this is set automatically
			// by your Activity's theme in your manifest. The provided system
			// theme Theme.WithActionBar enables this for you. Use it as you would
			// use Theme.NoTitleBar. You can add an Action Bar to your own themes
			// by adding the element <item name="android:windowActionBar">true</item>
			// to your style definition.

			//SupportRequestWindowFeature(WindowCompat.FeatureActionBar);


			_subnetMaskDropDown = FindViewById<MvxSpinner>(Resource.Id.SubnetMaskDropDown);
			_subnetMaskDropDown.ItemSelected += _subnetMaskDropDown_ItemSelected;
			_subnetMaskDropDown.SetSelection(23);

			var subnetsList = FindViewById<MvxListView>(Resource.Id.SubnetListView);

			subnetsList.ItemClick = new MvxCommand(SubnetList_ItemClick);

			////////////////////////////// InApp Billing ///////////////////////////////////
			// Create a new connection to the Google Play Service
			_serviceConnection = new InAppBillingServiceConnection(this, _publicKey);
			_serviceConnection.OnConnected += _serviceConnection_OnConnected;
			_serviceConnection.OnInAppBillingError += _serviceConnection_OnInAppBillingError;
			// Attempt to connect to the service
			_serviceConnection.Connect();

		}

		private void _serviceConnection_OnInAppBillingError(InAppBillingErrorType error, string message)
		{
			var toast = new Toast(this);
			toast.SetText(message);

			toast.Show();
		}

		private void _serviceConnection_OnConnected()
		{
			_serviceConnection.BillingHandler.OnGetProductsError += (responseCode, ownedItems) =>
			{
				Alert("Error getting products", "Error");
			};

			_serviceConnection.BillingHandler.OnInvalidOwnedItemsBundleReturned += (Bundle ownedItems) =>
			{
				Alert("Invalid owned items bundle returned", "Error");
			};

			_serviceConnection.BillingHandler.OnProductPurchasedError += (int responseCode, string sku) =>
			{
				Alert(string.Format("Error purchasing item {0}", sku), "Error");
			};

			_serviceConnection.BillingHandler.InAppBillingProcesingError += (message) =>
			{
				Alert(string.Format("In app billing processing error {0}", message), "Error");
			};

			_serviceConnection.BillingHandler.OnProductPurchaseCompleted += HandleOnProductPurchaseCompleted;

			// Load available products and any purchases
			// Remember to unbind from the In-app Billing service when you are 
			// done with your Activity. If you don’t unbind, the open service 
			// connection could cause your device’s performance to degrade.
			_products = _serviceConnection.BillingHandler.QueryInventoryAsync(new List<string> {
				PRODUCT_ID_REMOVE_ADS
			}, ItemType.Product).Result;


			// Were any products returned?
			if (_products == null)
			{
				return;
			}

			// Ask the open connection's billing handler to get any purchases
			var purchases = _serviceConnection.BillingHandler.GetPurchases(ItemType.Product);

			if (!purchases.Any(p => p.ProductId == PRODUCT_ID_REMOVE_ADS && p.PurchaseState == 0))
			{
				SetAds();
			}
		}

		void HandleOnProductPurchaseCompleted(int response, Purchase purchase)
		{
			if ((purchase.PurchaseState == 0 ||
				purchase.PurchaseState == 7) &&
				purchase.ProductId == PRODUCT_ID_REMOVE_ADS)
			{
				_adLayout.RemoveView(_ad);
			}
		}

		private void Alert(string message, string title = "Alert")
		{
			var dlg = new AlertDialog.Builder(this);
			dlg.SetMessage(message);
			dlg.SetTitle(title);
			dlg.SetPositiveButton("Ok", (s, e) => dlg.Dispose());
			dlg.Create().Show();
		}

		private void PurchaseProduct(Product product)
		{
			// Ask the open connection's billing handler to purchase the selected product
			_serviceConnection.BillingHandler.BuyProduct(product);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			// Ask the open service connection's billing handler to process this request
			_serviceConnection.BillingHandler.HandleActivityResult(requestCode, resultCode, data);

			// TODO: Use a call back to update the purchased items
			// or listen to the OnProductPurchaseCompleted event to
			// handle a successful purchase
		}

		protected override void OnDestroy()
		{

			// Are we attached to the Google Play Service?
			if (_serviceConnection != null)
			{
				// Yes, disconnect
				_serviceConnection.Disconnect();
			}

			// Call base method
			base.OnDestroy();
		}

		private void SetAds()
		{
			_ad = new AdView(this)
			{
				AdSize = AdSize.SmartBanner,
				AdUnitId = "ca-app-pub-9589034451822525/6315157897"
			};

			var requestbuilder = new AdRequest.Builder();
			_ad.LoadAd(requestbuilder.Build());
			_adLayout = FindViewById<LinearLayout>(Resource.Id.ad);
			_adLayout.AddView(_ad);
		}


		public void SubnetList_ItemClick()
		{
			//var subnetsList = FindViewById<MvxListView>(Resource.Id.SubnetListView);

			var viewModel = (MainViewModel)ViewModel;

			//var selectedItem = viewModel.Subnets[subnetsList.SelectedItemPosition];

			var selectedItem = viewModel.SelectedIpSegment;

			var dialog = new Dialog(this);
			dialog.SetContentView(Resource.Layout.SubnetDetails);

			var networkId = (TextView)dialog.FindViewById(Resource.Id.NetworkIdTextView);
			var broadcast = (TextView)dialog.FindViewById(Resource.Id.BroadcastTextView);
			var subnetMask = (TextView)dialog.FindViewById(Resource.Id.SubnetMaskTextView);
			var wilcardSubnetMask = (TextView)dialog.FindViewById(Resource.Id.WildcardMaskTextView);
			var cidr = (TextView)dialog.FindViewById(Resource.Id.CIDRTextView);
			var firstUsable = (TextView)dialog.FindViewById(Resource.Id.FirstUsableTextView);
			var lastUsable = (TextView)dialog.FindViewById(Resource.Id.LastUsableTextView);
			var numberOfHosts = (TextView)dialog.FindViewById(Resource.Id.NumberOfHostsTextView);
			var rfc3021 = (TextView)dialog.FindViewById(Resource.Id.RFC3021TextView);

			networkId.Text = selectedItem.NetworkAddress.ToIpString();
			broadcast.Text = selectedItem.BroadcastAddress.ToIpString();
			subnetMask.Text = selectedItem.SubnetMask;
			wilcardSubnetMask.Text = selectedItem.WildCardSubnetMask;
			cidr.Text = selectedItem.Cidr.ToString(CultureInfo.InvariantCulture);
			firstUsable.Text = selectedItem.FirstUsable.ToIpString();
			lastUsable.Text = selectedItem.LastUsable.ToIpString();
			numberOfHosts.Text = selectedItem.NumberOfHosts.ToString(CultureInfo.InvariantCulture);
			rfc3021.Text = selectedItem.Rfc3021.ToString();

			var button = (Button)dialog.FindViewById(Resource.Id.OkButton);
			button.Click += (s, ev) => dialog.Hide();

			dialog.SetTitle("Subnet details");
			dialog.Show();

		}

		private void ShowAboutDialog()
		{
			var dialog = new Dialog(this);
			dialog.SetContentView(Resource.Layout.AboutDialog);
			dialog.SetTitle("About");

			var button = (Button)dialog.FindViewById(Resource.Id.AboutOkButton);
			button.Click += (s, ev) => dialog.Hide();

			dialog.Show();
		}


		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			var viewModel = (MainViewModel)ViewModel;

			var listSubnets = menu.Add("List Subnets")
				.SetOnMenuItemClickListener(
								  new DelegatedMenuItemListener(m =>
								  {
									  viewModel.ListSubnetsCommand.Execute(null);
									  return true;
								  }))
				.SetTitle("List Subnets");
			MenuItemCompat.SetShowAsAction(listSubnets, MenuItemCompat.ShowAsActionAlways);

			var about = menu.Add("About")
				.SetOnMenuItemClickListener(
							new DelegatedMenuItemListener(m =>
							{
								ShowAboutDialog();
								return true;
							}))
				.SetTitle("About");
			MenuItemCompat.SetShowAsAction(about, MenuItemCompat.ShowAsActionNever);

			var removeAds = menu.Add("Remove Ads")
				.SetOnMenuItemClickListener(
								new DelegatedMenuItemListener(m =>
								{
									PurchaseProduct(_products.First());
									return true;
								}))
				.SetTitle("Remove Ads");
			MenuItemCompat.SetShowAsAction(removeAds, MenuItemCompat.ShowAsActionNever);

			//var review = menu.Add("Review")
			//    .SetOnMenuItemClickListener(
			//    new DelegatedMenuItemListener(m =>
			//    {

			//        return true;
			//    }))
			//    .SetTitle("Review");
			//MenuItemCompat.SetShowAsAction(review, MenuItemCompat.ShowAsActionIfRoom);

			return true;
		}

		private void _subnetMaskDropDown_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
		{
			var viewModel = (MainViewModel)ViewModel;
			viewModel.MaskSelectedCommand.Execute(null);
		}
	}
}