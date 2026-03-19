namespace MyFileExplorer
{
	/// <summary>
	/// In-app clipboard for Cut/Copy file and folder paths. Paste uses the current target folder.
	/// </summary>
	internal static class FileClipboard
	{
		private static readonly List<string> Paths = new();
		private static bool _isCut;

		public static IReadOnlyList<string> GetPaths() => Paths.AsReadOnly();
		public static bool IsCut => _isCut;
		public static bool HasPaths => Paths.Count > 0;

		public static void SetCopy(IEnumerable<string> paths)
		{
			Paths.Clear();
			Paths.AddRange(paths.Where(p => !string.IsNullOrWhiteSpace(p)));
			_isCut = false;
		}

		public static void SetCut(IEnumerable<string> paths)
		{
			Paths.Clear();
			Paths.AddRange(paths.Where(p => !string.IsNullOrWhiteSpace(p)));
			_isCut = true;
		}

		public static void Clear()
		{
			Paths.Clear();
			_isCut = false;
		}

		public static void PasteTo(string destinationFolder)
		{
			if (Paths.Count == 0 || string.IsNullOrWhiteSpace(destinationFolder))
				return;
			if (_isCut)
			{
				FileOperations.Move(Paths, destinationFolder);
				Clear();
			}
			else
				FileOperations.Copy(Paths, destinationFolder);
		}
	}
}
