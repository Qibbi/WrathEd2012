using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WrathEd
{
	public class WrathEdTreeViewItem : TreeViewItem
	{
		public const string IconUri = "pack://application:,,,/WrathEd;component/WrathEd.ico";
		public const string ImageUri = "pack://application:,,,/WrathEd;component/Art/Textures/Icon";

		public const string ContextNewStream = "New Stream...";
		public const string ContextNewAsset = "New Asset...";
		public const string ContextRefresh = "Refresh";
		public const string ContextShow = "Show in Windows Explorer";
		public const string ContextProperties = "Properties";
		public const string ContextDelete = "Delete";

		public const string DeleteFile = "Do you really wish to delete '{0}'?";
		public const string DeleteFileCap = "Delete File";

		public enum ItemType
		{
			PROJECT,
			STREAM_FOLDER,
			STREAM,
			DATA_FOLDER,
			ASSET,
			MISC_FOLDER,
			FOLDER,
			FILE
		}

		public string id { get; private set; }
		private ItemType Type { get; set; }
		private string Title { get; set; }
		private string Path { get; set; }

		private BitmapImage ExpandedImage { get; set; }
		private BitmapImage CollapsedImage { get; set; }

		public Func<bool> Refresh { get; private set; }
		public Func<bool> Properties { get; private set; }
		public Func<bool, bool> Delete { get; private set; }

		public WrathEdTreeViewItem(string name, string path, ItemType type)
			: base()
		{
			id = name;
			Path = path;
			switch (type)
			{
				case ItemType.PROJECT:
					ExpandedImage = new BitmapImage(new Uri(IconUri));
					CollapsedImage = new BitmapImage(new Uri(IconUri));
					CreateProjectItem();
					break;
				case ItemType.STREAM_FOLDER:
					ExpandedImage = new BitmapImage(new Uri(ImageUri + "FolderOpen.png"));
					CollapsedImage = new BitmapImage(new Uri(ImageUri + "Folder.png"));
					CreateStreamFolderItem();
					break;
				case ItemType.STREAM:
					ExpandedImage = new BitmapImage(new Uri(ImageUri + "Stream.png"));
					CollapsedImage = new BitmapImage(new Uri(ImageUri + "Stream.png"));
					CreateStreamItem();
					break;
				case ItemType.DATA_FOLDER:
					ExpandedImage = new BitmapImage(new Uri(ImageUri + "FolderOpen.png"));
					CollapsedImage = new BitmapImage(new Uri(ImageUri + "Folder.png"));
					break;
				case ItemType.ASSET:
					ExpandedImage = new BitmapImage(new Uri(ImageUri + "Asset.png"));
					CollapsedImage = new BitmapImage(new Uri(ImageUri + "Asset.png"));
					break;
				case ItemType.MISC_FOLDER:
					ExpandedImage = new BitmapImage(new Uri(ImageUri + "FolderOpen.png"));
					CollapsedImage = new BitmapImage(new Uri(ImageUri + "Folder.png"));
					CreateMiscFolderItem();
					break;
				case ItemType.FOLDER:
					ExpandedImage = new BitmapImage(new Uri(ImageUri + "FolderOpen.png"));
					CollapsedImage = new BitmapImage(new Uri(ImageUri + "Folder.png"));
					CreateFolderItem();
					break;
				case ItemType.FILE:
					ExpandedImage = new BitmapImage(new Uri(ImageUri + "File.png"));
					CollapsedImage = new BitmapImage(new Uri(ImageUri + "File.png"));
					CreateFileItem();
					break;
			}
			IsExpanded = true;
			Item_Expanded(null, null);
			Expanded += Item_Expanded;
			Collapsed += Item_Collapsed;
		}

		#region Project
		private void CreateProjectItem()
		{
			Title = string.Format("Project '{0}'", id);

			MouseDoubleClick += Item_DoubleClick;

			ContextMenu = new ContextMenu();

			MenuItem item = new MenuItem();
			item.Header = ContextRefresh;
			item.Click += Context_Refresh;
			ContextMenu.Items.Add(item);

			item = new MenuItem();
			item.Header = ContextShow;
			item.Click += Context_Show;
			ContextMenu.Items.Add(item);

			ContextMenu.Items.Add(new Separator());

			item = new MenuItem();
			item.Header = ContextProperties;
			item.Click += Context_Properties;
			ContextMenu.Items.Add(item);

			Refresh = RefreshProjectItem;
			Refresh();

			Properties = PropertiesProjectItem;
		}

		private bool RefreshProjectItem()
		{
			while (Items.Count > 0)
			{
				Items.RemoveAt(0);
			}
			Items.Add(new WrathEdTreeViewItem("Streams",
				string.Format("{0}{1}Streams", Path, System.IO.Path.DirectorySeparatorChar),
				ItemType.STREAM_FOLDER));
			Items.Add(new WrathEdTreeViewItem("Misc",
				string.Format("{0}{1}Misc", Path, System.IO.Path.DirectorySeparatorChar),
				ItemType.MISC_FOLDER));
			return true;
		}

		private bool PropertiesProjectItem()
		{
			//Globals.MainWindow.OpenProjectSettingsTab();
			return true;
		}
		#endregion

		#region StreamFolder
		private void CreateStreamFolderItem()
		{
			Title = id;

			ContextMenu = new ContextMenu();

			MenuItem item = new MenuItem();
			item.Header = ContextNewStream;
			item.Click += Context_NewStream;
			ContextMenu.Items.Add(item);

			ContextMenu.Items.Add(new Separator());

			item = new MenuItem();
			item.Header = ContextRefresh;
			item.Click += Context_Refresh;
			ContextMenu.Items.Add(item);

			item = new MenuItem();
			item.Header = ContextShow;
			item.Click += Context_Show;
			ContextMenu.Items.Add(item);

			Refresh = RefreshStreamFolderItem;
			Refresh();
		}

		private bool RefreshStreamFolderItem()
		{
			while (Items.Count > 0)
			{
				Items.RemoveAt(0);
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(Path);
			if (directoryInfo.Exists)
			{
				foreach (DirectoryInfo info in directoryInfo.GetDirectories())
				{
					Items.Add(new WrathEdTreeViewItem(info.Name, info.FullName, ItemType.STREAM));
				}
			}
			return true;
		}
		#endregion

		#region Stream
		private void CreateStreamItem()
		{
			Title = id;

			MouseDoubleClick += Item_DoubleClick;

			ContextMenu = new ContextMenu();

			MenuItem item = new MenuItem();
			item.Header = ContextNewAsset;
			item.Click += Context_NewAsset;
			ContextMenu.Items.Add(item);

			ContextMenu.Items.Add(new Separator());

			item = new MenuItem();
			item.Header = ContextRefresh;
			item.Click += Context_Refresh;
			ContextMenu.Items.Add(item);

			item = new MenuItem();
			item.Header = ContextShow;
			item.Click += Context_Show;
			ContextMenu.Items.Add(item);

			ContextMenu.Items.Add(new Separator());

			item = new MenuItem();
			item.Header = ContextDelete;
			item.Click += Context_Delete;
			ContextMenu.Items.Add(item);

			ContextMenu.Items.Add(new Separator());

			item = new MenuItem();
			item.Header = ContextProperties;
			item.Click += Context_Properties;
			ContextMenu.Items.Add(item);

			Refresh = RefreshStreamItem;
			Refresh();

			Delete = DeleteStreamItem;
			Properties = PropertiesStreamItem;
		}

		private bool RefreshStreamItem()
		{
			while (Items.Count > 0)
			{
				Items.RemoveAt(0);
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(Path);
			if (directoryInfo.Exists)
			{
				foreach (DirectoryInfo info in directoryInfo.GetDirectories())
				{
					Items.Add(new WrathEdTreeViewItem(info.Name, info.FullName, ItemType.DATA_FOLDER));
				}
			}
			return true;
		}

		private bool DeleteStreamItem(bool refreshParent)
		{
			return false;
		}

		private bool PropertiesStreamItem()
		{
			return true;
		}
		#endregion

		#region DataFolder
		#endregion

		#region Asset
		#endregion

		#region MiscFolder
		private void CreateMiscFolderItem()
		{
			Title = id;

			ContextMenu = new ContextMenu();

			MenuItem item = new MenuItem();
			item.Header = ContextRefresh;
			item.Click += Context_Refresh;
			ContextMenu.Items.Add(item);

			item = new MenuItem();
			item.Header = ContextShow;
			item.Click += Context_Show;
			ContextMenu.Items.Add(item);

			Refresh = RefreshMiscFolderItem;
			Refresh();
		}

		private bool RefreshMiscFolderItem()
		{
			while (Items.Count > 0)
			{
				Items.RemoveAt(0);
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(Path);
			if (directoryInfo.Exists)
			{
				foreach (DirectoryInfo info in directoryInfo.GetDirectories())
				{
					Items.Add(new WrathEdTreeViewItem(info.Name, info.FullName, ItemType.FOLDER));
				}
				foreach (FileInfo info in directoryInfo.GetFiles())
				{
					Items.Add(new WrathEdTreeViewItem(info.Name, info.FullName, ItemType.FILE));
				}
			}
			return true;
		}
		#endregion

		#region Folder
		private void CreateFolderItem()
		{
			Title = id;

			ContextMenu = new ContextMenu();

			MenuItem item = new MenuItem();
			item.Header = ContextRefresh;
			item.Click += Context_Refresh;
			ContextMenu.Items.Add(item);

			item = new MenuItem();
			item.Header = ContextShow;
			item.Click += Context_Show;
			ContextMenu.Items.Add(item);

			ContextMenu.Items.Add(new Separator());

			item = new MenuItem();
			item.Header = ContextDelete;
			item.Click += Context_Delete;
			ContextMenu.Items.Add(item);

			Refresh = RefreshFolderItem;
			Refresh();

			Delete = DeleteFolderItem;
		}

		private bool RefreshFolderItem()
		{
			while (Items.Count > 0)
			{
				Items.RemoveAt(0);
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(Path);
			if (directoryInfo.Exists)
			{
				foreach (DirectoryInfo info in directoryInfo.GetDirectories())
				{
					Items.Add(new WrathEdTreeViewItem(info.Name, info.FullName, ItemType.FOLDER));
				}
				foreach (FileInfo info in directoryInfo.GetFiles())
				{
					Items.Add(new WrathEdTreeViewItem(info.Name, info.FullName, ItemType.FILE));
				}
			}
			return true;
		}

		private bool DeleteFolderItem(bool refreshParent)
		{
			bool isSuccessfull = true;
			foreach (WrathEdTreeViewItem item in Items)
			{
				if (!item.Delete(false))
				{
					isSuccessfull = false;
					break;
				}
			}
			if (isSuccessfull)
			{
				try
				{
					Directory.Delete(Path);
				}
				catch
				{
					isSuccessfull = false;
				}
			}
			if (refreshParent)
			{
				(Parent as WrathEdTreeViewItem).Refresh();
			}
			return isSuccessfull;
		}
		#endregion

		#region File
		private void CreateFileItem()
		{
			Title = id;

			MouseDoubleClick += Item_DoubleClick;

			ContextMenu = new ContextMenu();

			MenuItem item = new MenuItem();
			item.Header = ContextShow;
			item.Click += Context_ShowFile;
			ContextMenu.Items.Add(item);

			ContextMenu.Items.Add(new Separator());
			
			item = new MenuItem();
			item.Header = ContextDelete;
			item.Click += Context_Delete;
			ContextMenu.Items.Add(item);

			Delete = DeleteFileItem;

			Properties = PropertiesFileItem;
		}

		private bool DeleteFileItem(bool refreshParent)
		{
			try
			{
				System.IO.File.Delete(Path);
			}
			catch
			{
				return false;
			}
			if (refreshParent)
			{
				(Parent as WrathEdTreeViewItem).Refresh();
			}
			return true;
		}

		private bool PropertiesFileItem()
		{
			ProcessStartInfo filePSI = new ProcessStartInfo(Path);
			Process.Start(filePSI);
			return true;
		}
		#endregion

		private void Item_DoubleClick(object sender, MouseButtonEventArgs args)
		{
			args.Handled = Properties();
		}

		private void Context_NewFolder(object sender, RoutedEventArgs args)
		{
		}

		private void Context_NewStream(object sender, RoutedEventArgs args)
		{
		}

		private void Context_NewAsset(object sender, RoutedEventArgs args)
		{
		}

		private void Context_Refresh(object sender, RoutedEventArgs args)
		{
			Refresh();
		}

		private void Context_Show(object sender, RoutedEventArgs args)
		{
			ProcessStartInfo explorerPSI = new ProcessStartInfo(Globals.ExplorerExe, Path);
			Process.Start(explorerPSI);
		}

		private void Context_ShowFile(object sender, RoutedEventArgs args)
		{
			ProcessStartInfo explorerPSI = new ProcessStartInfo(Globals.ExplorerExe, "/select," + Path);
			Process.Start(explorerPSI);
		}

		private void Context_Properties(object sender, RoutedEventArgs args)
		{
			Properties();
		}

		private void Context_Delete(object sender, RoutedEventArgs args)
		{
			MessageBoxResult mbr = MessageBox.Show(
				string.Format(DeleteFile, id),
				DeleteFileCap,
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);
			if (mbr == MessageBoxResult.Yes)
			{
				Delete(true);
			}
		}

		private void Item_Expanded(object sender, RoutedEventArgs args)
		{
			Grid grid = new Grid();
			ColumnDefinition cd = new ColumnDefinition();
			cd.Width = new GridLength(20);
			grid.ColumnDefinitions.Add(cd);
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			if (ExpandedImage != null)
			{
				Image image = new Image();
				image.Source = ExpandedImage;
				image.Width = 16;
				grid.Children.Add(image);
			}
			TextBlock tb = new TextBlock();
			tb.Text = Title;
			grid.Children.Add(tb);
			Grid.SetColumn(tb, 1);
			Header = grid;
			if (args != null)
			{
				args.Handled = true;
			}
		}

		private void Item_Collapsed(object sender, RoutedEventArgs args)
		{
			Grid grid = new Grid();
			ColumnDefinition cd = new ColumnDefinition();
			cd.Width = new GridLength(20);
			grid.ColumnDefinitions.Add(cd);
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			if (CollapsedImage != null)
			{
				Image image = new Image();
				image.Source = CollapsedImage;
				image.Width = 16;
				grid.Children.Add(image);
			}
			TextBlock tb = new TextBlock();
			tb.Text = Title;
			grid.Children.Add(tb);
			Grid.SetColumn(tb, 1);
			Header = grid;
			if (args != null)
			{
				args.Handled = true;
			}
		}
	}
}
