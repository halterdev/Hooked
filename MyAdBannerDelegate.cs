using System;
using MonoTouch.iAd;
using MonoTouch.UIKit;

namespace Hooked
{
	public class MyAdBannerDelegate : ADBannerViewDelegate
	{
		public MyAdBannerDelegate ()
		{

		}
			
		public override void AdLoaded(ADBannerView banner)
		{
			banner.Frame = new System.Drawing.RectangleF(0,430, banner.Frame.Width, banner.Frame.Height);
			banner.Hidden = false;
			//view.Add (banner);
		}

		public override void FailedToReceiveAd(ADBannerView banner, MonoTouch.Foundation.NSError e)
		{
			Console.WriteLine(e);
			banner.Hidden = false;
		}

		public override void ActionFinished(ADBannerView banner)
		{

		}

		public void HideAd(ADBannerView banner)
		{
			banner.Hidden = true;
		}

		public void ShowAd(ADBannerView banner)
		{
			banner.Hidden = false;
		}
	}
}

