using SRO.PK2;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Folder2Pk2
{
    internal class Program
    {
        #region App Setup
        private static string mClientPath = string.Empty;
        private static string mPk2Key = "169841";
        private static List<string> mFolders = new List<string>();
        #endregion
        /// <summary>
        /// Application entry point.
        /// </summary>
        private static void Main(string[] args)
        {
            Console.Title = "Folder2Pk2 v"+ FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion + " - https://github.com/JellyBitz/Folder2Pk2";
            Console.WriteLine(Console.Title + Environment.NewLine);

            LoadCommandLine(args);
            // Check if the minimum has been set up
            if (mFolders.Count == 0)
            {
                DisplayUsage();
                return;
            }

            // Set current path as client folder
            if (mClientPath == string.Empty)
            {
                Console.WriteLine(" (!) Client path hasn't been specified, using the current path as client." + Environment.NewLine);
                mClientPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            }

            // Try to import each folder into his respective .pk2 file
            foreach (var folder in mFolders)
            {
                var pk2Name = new DirectoryInfo(folder).Name;
                var pk2Path = Path.Combine(mClientPath, pk2Name) + ".pk2";

                // Check if the client contains the respective .pk2
                if (File.Exists(pk2Path))
                {
                    // Try to open it
                    try
                    {
                        using (var pk2 = new Pk2Stream(pk2Path, mPk2Key))
                        {
                            Console.WriteLine("Importing all into \"" + pk2Name + ".pk2\"..." + Environment.NewLine);
                            // Import all the files recursively
                            ImportAll(pk2, folder, string.Empty);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        DisplayPause();
                    }
                }
                else
                {
                    Console.WriteLine("Error: \"" + pk2Name + ".pk2\" not found at client path!");
                    DisplayPause();
                }
            }
        }
        /// <summary>
        /// Read and loads data from the command line arguments.
        /// </summary>
        private static void LoadCommandLine(string[] args)
        {
            foreach (var arg in args)
            {
                // Check commands
                if (arg.StartsWith("-client="))
                {
                    var path = arg.Substring(8);
                    // Make sure it exists
                    if (Directory.Exists(path))
                        mClientPath = path;
                }
                else if (arg.StartsWith("-key="))
                {
                    mPk2Key = arg.Substring(5);
                }
                else
                {
                    // Check if argument is path to a folder
                    if (Directory.Exists(arg))
                        mFolders.Add(arg);
                }
            }
        }
        /// <summary>
        /// Shows a quick info about command line usage.
        /// </summary>
        private static void DisplayUsage()
        {
            // Short description
            Console.WriteLine("Import entire folder content right into the .pk2 file with the same folder name.");
            Console.WriteLine("All this with just drag and drop the folder into the application.");
            Console.WriteLine();
            Console.WriteLine("Folder2Pk2 \"-client=C:\\Games\\Silkroad\" \"-key=169841\"");
            Console.WriteLine();
            Console.WriteLine(" -client= * Path to the client to import the folder");
            Console.WriteLine(" -key= * Encryption key used by the .pk2 file");
            Console.WriteLine();
            DisplayPause();
        }
        /// <summary>
        /// Mimic classic System("pause") from C++.
        /// </summary>
        private static void DisplayPause()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        /// <summary>
        /// Import files and folders from source path into stream, keeps the same order as the source path.
        /// </summary>
        private static void ImportAll(Pk2Stream stream, string sourcePath, string pk2Path)
        {
            // Creates folder just in case it is an empty folder
            stream.AddFolder(pk2Path);
            // Import each file
            foreach (string file in Directory.GetFiles(Path.Combine(sourcePath, pk2Path)))
            {
                var innerPath = new FileInfo(file).Name;
                if (!string.IsNullOrEmpty(pk2Path))
                    innerPath = Path.Combine(pk2Path, innerPath);
                // Load file and add it
                stream.AddFile(innerPath, File.ReadAllBytes(file));
                Console.WriteLine(" Imported: " + innerPath);
            }
            // Import each subfolder
            foreach (string folder in Directory.GetDirectories(Path.Combine(sourcePath, pk2Path)))
            {
                var innerPath = folder.Substring(sourcePath.Length + 1);
                ImportAll(stream, sourcePath, innerPath);
            }
        }
    }
}
