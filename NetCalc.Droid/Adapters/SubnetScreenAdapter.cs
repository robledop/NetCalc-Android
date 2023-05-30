using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using NetCalc.Core.Models;

namespace NetCalc.Droid.Adapters
{
	public class SubnetScreenAdapter : BaseAdapter<IpSegment>
	{
	    readonly List<IpSegment> _items;
	    readonly Activity _context;

		public SubnetScreenAdapter (Activity context, List<IpSegment> items)
		{
			_context = context;
			_items = items;
		}

		public override long GetItemId (int position)
		{

			return position;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var item = _items [position];
			View view = convertView;

			if (view == null) {
				view = _context.LayoutInflater.Inflate (Resource.Layout.ListViewTemplate2, null);
			}

			view.FindViewById<TextView> (Resource.Id.textView1).Text = string.Format ("Network: {0}, CIDR: {1}, Hosts: {2}", item.NetworkAddress.ToIpString(), item.Cidr, item.NumberOfHosts);

			return view;
		}

		public override int Count {
			get {
				return _items.Count;
			}
		}

		public override IpSegment this [int index] {
			get {
				return _items [index];
			}
		}
	}
}

