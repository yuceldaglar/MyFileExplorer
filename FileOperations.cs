using System.Runtime.InteropServices;

namespace MyFileExplorer
{
	/// <summary>
	/// File and folder operations: recycle bin delete, copy, move, and open with default app.
	/// </summary>
	internal static class FileOperations
	{
		private const int FO_DELETE = 0x0003;
		private const int FOF_ALLOWUNDO = 0x0040;
		private const int FOF_NOCONFIRMATION = 0x0010;
		private const int FOF_SILENT = 0x0044; // no UI

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct SHFILEOPSTRUCT
		{
			public IntPtr hwnd;
			public int wFunc;
			public string pFrom;
			public string pTo;
			public short fFlags;
			[MarshalAs(UnmanagedType.Bool)]
			public bool fAnyOperationsAborted;
			public IntPtr hNameMappings;
			public string? lpszProgressTitle;
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

		/// <summary>
		/// Move file or folder to the recycle bin. Uses SHFileOperation with FOF_ALLOWUNDO.
		/// </summary>
		public static bool RecycleBinDelete(string path)
		{
			if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path) && !File.Exists(path))
				return false;
			// pFrom must be double-null terminated
			var pFrom = path + '\0';
			var shf = new SHFILEOPSTRUCT
			{
				wFunc = FO_DELETE,
				pFrom = pFrom,
				pTo = "",
				fFlags = (short)(FOF_ALLOWUNDO | FOF_NOCONFIRMATION),
				fAnyOperationsAborted = false
			};
			return SHFileOperation(ref shf) == 0 && !shf.fAnyOperationsAborted;
		}

		/// <summary>
		/// Move multiple files/folders to the recycle bin. Paths separated by null, list double-null terminated.
		/// </summary>
		public static bool RecycleBinDelete(IEnumerable<string> paths)
		{
			var list = paths.Where(p => !string.IsNullOrWhiteSpace(p) && (Directory.Exists(p) || File.Exists(p))).ToList();
			if (list.Count == 0)
				return false;
			var pFrom = string.Join("\0", list) + "\0\0";
			var shf = new SHFILEOPSTRUCT
			{
				wFunc = FO_DELETE,
				pFrom = pFrom,
				pTo = "",
				fFlags = (short)(FOF_ALLOWUNDO | FOF_NOCONFIRMATION),
				fAnyOperationsAborted = false
			};
			return SHFileOperation(ref shf) == 0 && !shf.fAnyOperationsAborted;
		}

		/// <summary>
		/// Open a file or folder with the default application (e.g. Explorer for folders).
		/// </summary>
		public static void Open(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return;
			try
			{
				using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = path,
					UseShellExecute = true
				});
			}
			catch
			{
				// Ignore (e.g. no associated app)
			}
		}

		/// <summary>
		/// Copy files or folders to the destination directory.
		/// </summary>
		public static void Copy(IEnumerable<string> sourcePaths, string destinationDirectory)
		{
			var dest = destinationDirectory?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (string.IsNullOrEmpty(dest) || !Directory.Exists(dest))
				return;
			foreach (var src in sourcePaths)
			{
				if (string.IsNullOrWhiteSpace(src))
					continue;
				var name = Path.GetFileName(src.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
				var destPath = Path.Combine(dest, name);
				try
				{
					if (Directory.Exists(src))
						CopyDirectory(src, destPath);
					else if (File.Exists(src))
						File.Copy(src, GetUniquePath(destPath));
				}
				catch
				{
					// Continue with next
				}
			}
		}

		/// <summary>
		/// Move files or folders to the destination directory.
		/// </summary>
		public static void Move(IEnumerable<string> sourcePaths, string destinationDirectory)
		{
			var dest = destinationDirectory?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (string.IsNullOrEmpty(dest) || !Directory.Exists(dest))
				return;
			foreach (var src in sourcePaths)
			{
				if (string.IsNullOrWhiteSpace(src))
					continue;
				var name = Path.GetFileName(src.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
				var destPath = Path.Combine(dest, name);
				try
				{
					if (Directory.Exists(src))
						Directory.Move(src, GetUniquePath(destPath));
					else if (File.Exists(src))
						File.Move(src, GetUniquePath(destPath));
				}
				catch
				{
					// Continue with next
				}
			}
		}

		private static void CopyDirectory(string sourceDir, string destDir)
		{
			var dir = new DirectoryInfo(sourceDir);
			if (!dir.Exists)
				return;
			destDir = GetUniquePath(destDir);
			Directory.CreateDirectory(destDir);
			foreach (var file in dir.GetFiles())
				file.CopyTo(Path.Combine(destDir, file.Name), overwrite: false);
			foreach (var sub in dir.GetDirectories())
				CopyDirectory(sub.FullName, Path.Combine(destDir, sub.Name));
		}

		private static string GetUniquePath(string path)
		{
			if (!File.Exists(path) && !Directory.Exists(path))
				return path;
			var dir = Path.GetDirectoryName(path) ?? "";
			var nameWithoutExt = Path.GetFileNameWithoutExtension(path);
			var ext = Path.GetExtension(path);
			var isDir = Directory.Exists(path) || string.IsNullOrEmpty(ext) && nameWithoutExt.Length > 0;
			for (int i = 1; ; i++)
			{
				var candidate = isDir
					? Path.Combine(dir, $"{nameWithoutExt} ({i})")
					: Path.Combine(dir, $"{nameWithoutExt} ({i}){ext}");
				if (!File.Exists(candidate) && !Directory.Exists(candidate))
					return candidate;
			}
		}
	}
}
