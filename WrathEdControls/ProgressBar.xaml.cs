#define NOT_USING_RECOLOR_SHADER

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace WrathEd.Controls
{
	public partial class ProgressBar : UserControl
	{
		public enum ProgressBarColor
		{
			RED,
			GREEN,
			BLUE
		}

#if USING_RECOLOR_SHADER
		private const string ActiveUri = "pack://application:,,,/WrathEdControls;component/Art/Textures/ProgressBar/ProgressBarTileActive.png";
		private const string InactiveUri = "pack://application:,,,/WrathEdControls;component/Art/Textures/ProgressBar/ProgressBarTileInactive.png";
#else
		private const string ActiveUri = "pack://application:,,,/WrathEdControls;component/Art/Textures/ProgressBar/ProgressBarTileActive_{0}.png";
		private const string InactiveUri = "pack://application:,,,/WrathEdControls;component/Art/Textures/ProgressBar/ProgressBarTileInactive_{0}.png";
#endif

		private int min;
		private int max;
		private static readonly DependencyProperty progressValueProperty =
			DependencyProperty.Register("ProgressValue", typeof(double), typeof(ProgressBar), new PropertyMetadata(0.0, NoProgressValueChanged));
		private double progress;
		private bool isProgress;
		public int NumTiles { get; private set; }

		private DoubleAnimation valueAnimation;

		private BitmapImage ActiveImage;
		private BitmapImage InactiveImage;

#if USING_RECOLOR_SHADER
		private Color color;
#else
		private ProgressBarColor color;
#endif

		public ProgressBar()
		{
			Width = 300;
			Height = 27;
			InitializeComponent();

			IsProgress = true;
			Min = 0;
			Max = 100;

			valueAnimation = new DoubleAnimation();
			valueAnimation.From = Min;
			valueAnimation.To = Max;
			valueAnimation.Duration = new Duration(TimeSpan.FromSeconds(2));
			valueAnimation.RepeatBehavior = RepeatBehavior.Forever;
			BeginAnimation(progressValueProperty, valueAnimation);

#if USING_RECOLOR_SHADER
			Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
#else
			Color = ProgressBarColor.RED;
#endif
		}

		public ProgressBar(int width, int height)
		{
			Width = width;
			Height = height;
			InitializeComponent();

			IsProgress = true;
			Min = 0;
			Max = 100;

			valueAnimation = new DoubleAnimation();
			valueAnimation.From = Min;
			valueAnimation.To = Max;
			valueAnimation.Duration = new Duration(TimeSpan.FromSeconds(2));
			valueAnimation.RepeatBehavior = RepeatBehavior.Forever;
			BeginAnimation(progressValueProperty, valueAnimation);

#if USING_RECOLOR_SHADER
			Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
#else
			Color = ProgressBarColor.RED;
#endif
		}

		public int Min
		{
			get
			{
				return min;
			}
			set
			{
				min = Math.Min(max, value);
				Value = Value;
			}
		}

		public int Max
		{
			get
			{
				return max;
			}
			set
			{
				max = Math.Max(min, value);
				Value = Value;
			}
		}

		public double Value
		{
			get
			{
				return progress;
			}
			set
			{
				progress = Math.Min(Math.Max(max, value), value);
				if (IsProgress)
				{
					double percentage = (Value - Min) / (Max - Min);
					int numActive = (int)Math.Round(NumTiles * percentage);
					for (int idx = 0; idx < NumTiles; ++idx)
					{
						if (idx <= numActive - 1)
						{
							(BackgroundGrid.Children[idx * 2 + 1] as Image).Opacity = 1.0;
						}
						else
						{
							(BackgroundGrid.Children[idx * 2 + 1] as Image).Opacity = 0.0;
						}
					}
				}
				else
				{
					double percentage = Value / 100;
					int numActive = (int)(NumTiles * percentage);
					for (int idx = 0; idx < NumTiles; ++idx)
					{
						if (idx == numActive)
						{
							(BackgroundGrid.Children[idx * 2 + 1] as Image).Opacity = 1.0;
						}
						else if (idx == numActive - 1 || idx == numActive + 1)
						{
							(BackgroundGrid.Children[idx * 2 + 1] as Image).Opacity = 0.5;
						}
						else
						{
							(BackgroundGrid.Children[idx * 2 + 1] as Image).Opacity = 0.0;
						}
					}
				}
			}
		}

		public bool IsProgress
		{
			get
			{
				return isProgress;
			}
			set
			{
				isProgress = value;
			}
		}

		public TimeSpan Duration
		{
			get
			{
				return valueAnimation.Duration.TimeSpan;
			}
			set
			{
				valueAnimation.Duration = new Duration(value);
				BeginAnimation(progressValueProperty, valueAnimation);
			}
		}

#if USING_RECOLOR_SHADER
		public Color Color
#else
		public ProgressBarColor Color
#endif
		{
			get
			{
				return color;
			}
			set
			{
				color = value;
				Refresh();
			}
		}

		private void Refresh()
		{
#if USING_RECOLOR_SHADER
			ActiveImage = new BitmapImage(new Uri(ActiveUri));
			InactiveImage = new BitmapImage(new Uri(InactiveUri));
#else
			ActiveImage = new BitmapImage(new Uri(string.Format(ActiveUri, Enum.GetName(typeof(ProgressBarColor), color))));
			InactiveImage = new BitmapImage(new Uri(string.Format(InactiveUri, Enum.GetName(typeof(ProgressBarColor), color))));
#endif
			if (BackgroundGrid.Children.Count > 0)
			{
				BackgroundGrid.Children.RemoveRange(0, BackgroundGrid.Children.Count);
			}
			if (BackgroundGrid.ColumnDefinitions.Count > 0)
			{
				BackgroundGrid.ColumnDefinitions.RemoveRange(0, BackgroundGrid.ColumnDefinitions.Count);
			}
			int numTiles = (int)Width / 17;
			int border = (int)Width % 34;
			NumTiles = numTiles - 1;
			ColumnDefinition column = new ColumnDefinition();
			column.Width = new GridLength(border / 3);
			BackgroundGrid.ColumnDefinitions.Add(column);
			for (int idx = 0; idx < NumTiles; ++idx)
			{
				column = new ColumnDefinition();
				column.Width = new GridLength(17);
				BackgroundGrid.ColumnDefinitions.Add(column);
			}
			column = new ColumnDefinition();
			BackgroundGrid.ColumnDefinitions.Add(column);
			for (int idx = 0; idx < NumTiles; ++idx)
			{
				Image image = new Image();
				image.Source = InactiveImage;
				image.Width = 34;
				image.Height = 27;
#if USING_RECOLOR_SHADER
				image.Effect = new WrathEd.Controls.Shader.ImageRecolor(color);
#endif
				BackgroundGrid.Children.Add(image);
				Grid.SetColumn(image, idx + 1);
				Grid.SetColumnSpan(image, 2);
				image = new Image();
				image.Source = ActiveImage;
				image.Width = 34;
				image.Height = 27;
				image.Opacity = 0.0;
#if USING_RECOLOR_SHADER
				image.Effect = new WrathEd.Controls.Shader.ImageRecolor(color);
#endif
				BackgroundGrid.Children.Add(image);
				Grid.SetColumn(image, idx + 1);
				Grid.SetColumnSpan(image, 2);
			}
		}

		private static void NoProgressValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			ProgressBar bar = d as ProgressBar;
			if (!bar.IsProgress)
			{
				bar.Value = (double)args.NewValue;
			}
		}

		private void ProgressBar_SizeChanged(object sender, SizeChangedEventArgs args)
		{
			Refresh();
		}
	}
}
