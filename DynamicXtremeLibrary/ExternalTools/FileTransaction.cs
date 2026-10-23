namespace DynamicXtremeLibrary.ExternalTools
{
    /// <summary>
    /// Tracks file and directory changes so an installation can either commit all modifications
    /// or restore the original file-system state.
    /// </summary>
    public sealed class FileTransaction : IDisposable
    {
        private readonly Dictionary<string, byte[]?> _originalFiles =
            new(InputPath.Comparer);
        private readonly List<string> _createdDirectories = [];
        private bool _completed;

        /// <summary>
        /// Writes text to a file after recording its original state for rollback.
        /// </summary>
        /// <param name="path">The file to write.</param>
        /// <param name="content">The complete replacement text.</param>
        public void WriteAllText(string path, string content)
        {
            rememberFile(path);
            ensureDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Writes binary content after recording the original file and optionally stores the
        /// original content at a backup path.
        /// </summary>
        /// <param name="path">The file to write.</param>
        /// <param name="content">The complete replacement content.</param>
        /// <param name="backupPath">The optional path that receives the original content.</param>
        public void WriteAllBytes(string path, byte[] content, string? backupPath = null)
        {
            string fullPath = Path.GetFullPath(path);
            rememberFile(fullPath);
            if (!string.IsNullOrWhiteSpace(backupPath))
            {
                rememberFile(backupPath);
                if (_originalFiles[fullPath] != null)
                {
                    ensureDirectory(Path.GetDirectoryName(Path.GetFullPath(backupPath))!);
                    File.WriteAllBytes(backupPath, _originalFiles[fullPath]!);
                }
            }
            ensureDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllBytes(fullPath, content);
        }

        /// <summary>
        /// Copies a file into the transaction after recording the destination's original state.
        /// </summary>
        /// <param name="source">The existing file to copy.</param>
        /// <param name="destination">The destination file that may be restored during rollback.</param>
        public void CopyFile(string source, string destination)
        {
            if (!File.Exists(source))
                throw new FileNotFoundException("Transaction copy source was not found", source);
            rememberFile(destination);
            ensureDirectory(Path.GetDirectoryName(Path.GetFullPath(destination))!);
            File.Copy(source, destination, true);
        }

        /// <summary>
        /// Creates a directory and records every newly created parent for possible rollback.
        /// </summary>
        /// <param name="path">The directory that must exist.</param>
        public void EnsureDirectory(string path) => ensureDirectory(path);

        /// <summary>
        /// Accepts every tracked change and prevents automatic rollback during disposal.
        /// </summary>
        public void Commit() => _completed = true;

        /// <summary>
        /// Restores modified files, removes newly created files, and removes empty directories
        /// created by this transaction.
        /// </summary>
        public void Rollback()
        {
            if (_completed)
                return;
            foreach (var original in _originalFiles.Reverse())
            {
                if (original.Value == null)
                {
                    if (File.Exists(original.Key))
                        File.Delete(original.Key);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(original.Key)!);
                    File.WriteAllBytes(original.Key, original.Value);
                }
            }
            foreach (string directory in _createdDirectories
                .OrderByDescending(path => path.Length))
            {
                if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
                    Directory.Delete(directory);
            }
            _completed = true;
        }

        /// <summary>
        /// Rolls back the transaction unless it has already been committed or rolled back.
        /// </summary>
        public void Dispose()
        {
            if (!_completed)
                Rollback();
        }

        /// <summary>
        /// Captures a file's original bytes once, using <see langword="null"/> to represent a path
        /// that did not exist when first touched.
        /// </summary>
        /// <param name="path">The file whose original state must be retained.</param>
        private void rememberFile(string path)
        {
            string fullPath = Path.GetFullPath(path);
            if (_originalFiles.ContainsKey(fullPath))
                return;
            _originalFiles.Add(fullPath, File.Exists(fullPath)
                ? File.ReadAllBytes(fullPath)
                : null);
        }

        /// <summary>
        /// Creates a directory tree and records only the directories created by this transaction so
        /// they can be removed in deepest-first order during rollback.
        /// </summary>
        /// <param name="path">The directory tree that must exist.</param>
        private void ensureDirectory(string path)
        {
            string fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath))
                return;
            Stack<string> missing = [];
            string? current = fullPath;
            while (!string.IsNullOrEmpty(current) && !Directory.Exists(current))
            {
                missing.Push(current);
                current = Path.GetDirectoryName(current);
            }
            while (missing.TryPop(out string? directory))
            {
                Directory.CreateDirectory(directory);
                _createdDirectories.Add(directory);
            }
        }
    }
}
