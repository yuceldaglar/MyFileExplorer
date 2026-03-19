using System.Runtime.InteropServices;

namespace MyFileExplorer
{
	internal static class ShellIconHelper
	{
		private const uint SHGFI_ICON = 0x000000100;
		private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
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
			largeList.Images.Clear();
			smallList.Images.Clear();

			// Folder: use a real folder path so we get the correct shell icon.
			var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			var addedFolder = AddIconToLists(folderPath, FILE_ATTRIBUTE_DIRECTORY, useFileAttributes: false, largeList, smallList);

			// File: use extension + SHGFI_USEFILEATTRIBUTES to get generic file icon
			// without requiring an existing file on disk.
			const string filePath = ".txt";
			var addedFile = AddIconToLists(filePath, FILE_ATTRIBUTE_NORMAL, useFileAttributes: true, largeList, smallList);

			// Keep indices stable for FolderContentsControl:
			// 0 => folder, 1 => file
			if (!addedFolder)
			{
				largeList.Images.Add(SystemIcons.WinLogo);
				smallList.Images.Add(SystemIcons.WinLogo);
			}

			if (!addedFile)
			{
				largeList.Images.Add(SystemIcons.Application);
				smallList.Images.Add(SystemIcons.Application);
			}
		}

		/// <summary>
		/// Adds the shell icon for a file extension to both image lists.
		/// Returns true when both large and small icons were added.
		/// </summary>
		public static bool AddFileTypeIcon(string extensionOrFileName, ImageList largeList, ImageList smallList)
		{
			if (largeList == null || smallList == null)
				return false;

			var token = extensionOrFileName?.Trim() ?? string.Empty;
			if (string.IsNullOrEmpty(token))
				return false;

			// For extension-based shell icon lookup with SHGFI_USEFILEATTRIBUTES, ".ext" is sufficient.
			if (!token.Contains(Path.DirectorySeparatorChar) &&
				!token.Contains(Path.AltDirectorySeparatorChar) &&
				!token.Contains('.') )
			{
				token = "." + token;
			}

			return AddIconToLists(token, FILE_ATTRIBUTE_NORMAL, useFileAttributes: true, largeList, smallList);
		}

		private static bool AddIconToLists(string path, uint fileAttributes, bool useFileAttributes, ImageList largeList, ImageList smallList)
		{
			var addedLarge = false;
			var addedSmall = false;
			var baseFlags = SHGFI_ICON | (useFileAttributes ? SHGFI_USEFILEATTRIBUTES : 0);

			var shfi = new SHFILEINFO();
			// Large icon
			_ = SHGetFileInfo(path, fileAttributes, ref shfi, (uint)Marshal.SizeOf(shfi), baseFlags | SHGFI_LARGEICON);
			if (shfi.hIcon != IntPtr.Zero)
			{
				try
				{
					using var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
					largeList.Images.Add(icon);
					addedLarge = true;
				}
				finally
				{
					DestroyIcon(shfi.hIcon);
				}
			}
			// Small icon
			shfi = new SHFILEINFO();
			_ = SHGetFileInfo(path, fileAttributes, ref shfi, (uint)Marshal.SizeOf(shfi), baseFlags | SHGFI_SMALLICON);
			if (shfi.hIcon != IntPtr.Zero)
			{
				try
				{
					using var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
					smallList.Images.Add(icon);
					addedSmall = true;
				}
				finally
				{
					DestroyIcon(shfi.hIcon);
				}
			}

			return addedLarge && addedSmall;
		}
	}
}
