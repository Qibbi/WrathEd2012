using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace WrathEd.Controls.Shader
{
	public class ButtonScroll : ShaderEffect
	{
		public static readonly DependencyProperty InputProperty =
			ShaderEffect.RegisterPixelShaderSamplerProperty(
			"Input", typeof(ButtonScroll), 0);
		public Brush Input
		{
			get { return (Brush)GetValue(InputProperty); }
			set { SetValue(InputProperty, value); }
		}

		public static readonly DependencyProperty alphaTex =
			ShaderEffect.RegisterPixelShaderSamplerProperty(
			"AlphaTex", typeof(ButtonScroll), 1);
		public Brush AlphaTex
		{
			get { return (Brush)GetValue(alphaTex); }
			set { SetValue(alphaTex, value); }
		}

		public static readonly DependencyProperty scrollTex =
			ShaderEffect.RegisterPixelShaderSamplerProperty(
			"ScrollTex", typeof(ButtonScroll), 2);
		public Brush ScrollTex
		{
			get { return (Brush)GetValue(scrollTex); }
			set { SetValue(scrollTex, value); }
		}

		public static readonly DependencyProperty IsMouseOver =
			DependencyProperty.Register("bIsMouseOver", typeof(float), typeof(ButtonScroll),
			new UIPropertyMetadata(0.0f, PixelShaderConstantCallback(0)));
		public float bIsMouseOver
		{
			get { return (float)GetValue(IsMouseOver); }
			set { SetValue(IsMouseOver, value); }
		}

		public static readonly DependencyProperty time =
			DependencyProperty.Register("Time", typeof(float), typeof(ButtonScroll),
			new UIPropertyMetadata(0.0f, PixelShaderConstantCallback(1)));
		public float Time
		{
			get { return (float)GetValue(time); }
			set { SetValue(time, value); }
		}

		public ButtonScroll()
		{
			PixelShader = new PixelShader();
			PixelShader.UriSource = new Uri("pack://application:,,,/WrathEdControls;component/Shader/ButtonScroll.ps");
			AlphaTex = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/WrathEdControls;component/Art/Textures/ShellButton/ShellButtonHoverAlpha.png")));
			ScrollTex = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/WrathEdControls;component/Art/Textures/ShellButton/ShellButtonHoverScroll.png")));
			UpdateShaderValue(InputProperty);
			UpdateShaderValue(alphaTex);
			UpdateShaderValue(scrollTex);
			SingleAnimation animation = new SingleAnimation();
			animation.From = 0.5f;
			animation.To = -1;
			animation.Duration = new Duration(TimeSpan.FromSeconds(1));
			animation.RepeatBehavior = RepeatBehavior.Forever;
			BeginAnimation(time, animation);
		}
	}
}
