using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;

namespace WrathEd.Controls.Shader
{
	public class ImageRecolor : ShaderEffect
	{
		public static readonly DependencyProperty InputProperty =
			ShaderEffect.RegisterPixelShaderSamplerProperty(
			"Input", typeof(ImageRecolor), 0);
		public Brush Input
		{
			get { return (Brush)GetValue(InputProperty); }
			set { SetValue(InputProperty, value); }
		}

		public static readonly DependencyProperty RecolorColorProperty =
			DependencyProperty.Register("RecolorColor", typeof(Color), typeof(ImageRecolor),
			new UIPropertyMetadata(new Color(), PixelShaderConstantCallback(0)));
		public Color RecolorColor
		{
			get { return (Color)GetValue(RecolorColorProperty); }
			set { SetValue(RecolorColorProperty, value); }
		}

		public ImageRecolor()
		{
			PixelShader = new PixelShader();
			PixelShader.UriSource = new Uri("pack://application:,,,/WrathEdControls;component/Shader/ImageRecolor.ps");
			UpdateShaderValue(InputProperty);
			RecolorColor = Color.FromRgb(0xFF, 0xFF, 0xFF);
		}

		public ImageRecolor(Color color)
		{
			PixelShader = new PixelShader();
			PixelShader.UriSource = new Uri("pack://application:,,,/WrathEdControls;component/Shader/ImageRecolor.ps");
			UpdateShaderValue(InputProperty);
			RecolorColor = color;
		}
	}
}
