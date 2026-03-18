using System.Runtime.InteropServices;

namespace MyFileExplorer
{
	internal static class ShellIconHelper
	{
		private const uint SHGFI_ICON = 0x000000100;
		private const uint SHGFI_LARGEICON = 0x000000000;
		private const uint SHGFI_SMALLICON = 0x000000001;
		private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
		private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool DestroyIcon(IntPtr hIcon);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}

		/// <summary>
		/// Adds folder icon at index 0 and file icon at index 1 to both image lists (large and small).
		/// </summary>
		public static void AddFolderAndFileIcons(ImageList largeList, ImageList smallList)
		{
			// Folder: use a real folder path so we get the correct shell icon
			var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			AddIconToLists(folderPath, FILE_ATTRIBUTE_DIRECTORY, largeList, smallList);

			// File: use a dummy .txt path to get generic document icon
			var filePath = Path.Combine(Path.GetTempPath(), "dummy.txt");
			AddIconToLists(filePath, FILE_ATTRIBUTE_NORMAL, largeList, smallList);
		}

		private static void AddIconToLists(string path, uint fileAttributes, ImageList largeList, ImageList smallList)
		{
			var shfi = new SHFILEINFO();
			// Large icon
			_ = SHGetFileInfo(path, fileAttributes, ref shfi, (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_LARGEICON);
			if (shfi.hIcon != IntPtr.Zero)
			{
				try
				{
					using var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
					largeList.Images.Add(icon);
				}
				finally
				{
					DestroyIcon(shfi.hIcon);
				}
			}
			// Small icon
			shfi = new SHFILEINFO();
			_ = SHGetFileInfo(path, fileAttributes, ref shfi, (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_SMALLICON);
			if (shfi.hIcon != IntPtr.Zero)
			{
				try
				{
					using var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
					smallList.Images.Add(icon);
				}
				finally
				{
					DestroyIcon(shfi.hIcon);
				}
			}
		}
	}
}
