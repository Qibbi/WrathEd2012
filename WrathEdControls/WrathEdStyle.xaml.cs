using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WrathEd.Controls.Shader;

namespace WrathEd.Controls
{
	public partial class WrathEdStyle : ResourceDictionary
	{
		private void Button_MouseEnter(object sender, MouseEventArgs args)
		{
			Button myButton = sender as Button;
			if (myButton.IsEnabled)
			{
				if (myButton.Effect == null)
				{
					myButton.Effect = new ButtonScroll();
				}
				ButtonScroll fx = myButton.Effect as ButtonScroll;
				fx.bIsMouseOver = 1;
			}
		}

		private void Button_MouseLeave(object sender, MouseEventArgs args)
		{
			Button myButton = sender as Button;
			if (myButton.IsEnabled && myButton.Effect != null)
			{
				ButtonScroll fx = myButton.Effect as ButtonScroll;
				fx.bIsMouseOver = 0;
			}
		}
	}
}