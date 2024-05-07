namespace ConsoleAppRetry
{
    internal class Program
    {
        private static DirectoryInfo _rootDirectory;
        private static string[] _specDirectory = new string[] { "Изображения", "Документы", "Прочее" };
        private static int _imagesCount = 0, _documentCount = 0, _othersCount = 0;
        static void Main(string[] args)
        {
            Console.WriteLine("Введите путь к диску: ");
            string directoryPath = Console.ReadLine();

            var driverInfo = new DriveInfo(directoryPath);
            Console.WriteLine($"Информации о диске: {driverInfo.VolumeLabel}, всего {driverInfo.TotalSize / 1024 / 1024} МБ, " +
                $"свободно {driverInfo.AvailableFreeSpace / 1024 / 1024} мб");

            _rootDirectory = driverInfo.RootDirectory;
            SearchDirectories(_rootDirectory);

            foreach( var directory in _rootDirectory.GetDirectories())
            {
                if (!_specDirectory.Contains(directory.Name))
                    directory.Delete(true);
            }

            var resultText = $"Всего обработано: {_imagesCount + _documentCount + _othersCount} файлов. " 
                + $"Из них { _imagesCount} изображений, {_documentCount} документов, {_othersCount} прочих файлов";

            Console.WriteLine(resultText);

            File.WriteAllText(_rootDirectory + "\\Инфо.txt", resultText);

            Console.ReadLine();
        }

        private static void SearchDirectories(DirectoryInfo currentDirectory)
        {
            if (!_specDirectory.Contains(currentDirectory.Name)){
                FilterFiles(currentDirectory);
                foreach (var chieldDirectory in currentDirectory.GetDirectories())
                {
                    SearchDirectories(chieldDirectory);
                }
            }
        }

        private static void FilterFiles(DirectoryInfo currentDirectory) 
        {
            var currentFiles = currentDirectory.GetFiles();

            foreach (var fileInfo in currentFiles)
            {
                if(new string[] {".jpg", ".jpeg", ".png", ".gif", ".tiff", ".bmp", ".svg" }.Contains(fileInfo.Extension.ToLower()))
                {
                    var PhotoDirectory = new DirectoryInfo(_rootDirectory + $"{_specDirectory[0]}\\");
                    if (!PhotoDirectory.Exists)
                        PhotoDirectory.Create();

                    var yearDirectory = new DirectoryInfo(PhotoDirectory + $"{fileInfo.LastWriteTime.Date.Year}\\");
                    if(!yearDirectory.Exists)
                        yearDirectory.Create();

                    MoveFiles(fileInfo, yearDirectory);
                    _imagesCount++;
                }
                else if(new string[] { ".doc", ".docx", ".pdf", ".xls", "xlsx", ".ppt", ".pptx" }.Contains(fileInfo.Extension.ToLower())){

                    var documentDirectory = new DirectoryInfo(_rootDirectory + $"{_specDirectory[1]}\\");
                    if (!documentDirectory.Exists)
                        documentDirectory.Create();

                    DirectoryInfo lenghtDirectory = null;

                    if (fileInfo.Length / 1024 / 1024 < 1)
                        lenghtDirectory = new DirectoryInfo(documentDirectory + "Менее 1 Мб\\");
                    else if (fileInfo.Length / 1024 / 1024 < 10)
                        lenghtDirectory = new DirectoryInfo(documentDirectory + "Более 10 Мб\\");
                    else
                        lenghtDirectory = new DirectoryInfo(documentDirectory + "От 1 до 10 Мб\\");

                    if (!lenghtDirectory.Exists)
                        lenghtDirectory.Create();

                    MoveFiles(fileInfo, lenghtDirectory);
                    _documentCount++;
                }
                else
                {
                    var othersDirectory = new DirectoryInfo(_rootDirectory + $"{_specDirectory[2]}\\");

                    if(!othersDirectory.Exists) 
                        othersDirectory.Create();

                    MoveFiles(fileInfo, othersDirectory);
                    _othersCount++;
                }
            }
        }

        private static void MoveFiles(FileInfo fileInfo, DirectoryInfo directoryInfo) 
        {
            var newFileInfo = new FileInfo(directoryInfo + $"\\{fileInfo.Name}");

            while (newFileInfo.Exists)
            {
                newFileInfo = new FileInfo(directoryInfo + $"\\{Path.GetFileNameWithoutExtension(fileInfo.FullName)} (1)" +
                    $"{newFileInfo.Extension}");
            }
            fileInfo.MoveTo(newFileInfo.FullName);
        }
    }
}
