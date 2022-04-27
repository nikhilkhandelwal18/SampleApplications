﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class QueryDuplicateFileNames
    {
        static void Main(string[] args)
        {
            // Uncomment QueryDuplicates2 to run that query.  
            QueryDuplicates();
            // QueryDuplicates2();  

            // Keep the console window open in debug mode.  
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static void QueryDuplicates()
        {
            // Change the root drive or folder if necessary  
            string startFolder = @"C:\Temp\";

            // Take a snapshot of the file system.  
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(startFolder);

            // This method assumes that the application has discovery permissions  
            // for all folders under the specified path.  
            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            // used in WriteLine to keep the lines shorter  
            int charsToSkip = startFolder.Length;

            // var can be used for convenience with groups.  
            var queryDupNames =
                from file in fileList
                group file.FullName.Substring(charsToSkip) by file.Name into fileGroup
                where fileGroup.Count() > 1
                select fileGroup;

            // Pass the query to a method that will  
            // output one page at a time.  
            PageOutput<string, string>(queryDupNames);
        }

        // A Group key that can be passed to a separate method.  
        // Override Equals and GetHashCode to define equality for the key.  
        // Override ToString to provide a friendly name for Key.ToString()  
        class PortableKey
        {
            public string Name { get; set; }
            public DateTime LastWriteTime { get; set; }
            public long Length { get; set; }

            public override bool Equals(object obj)
            {
                PortableKey other = (PortableKey)obj;
                return other.LastWriteTime == this.LastWriteTime &&
                       other.Length == this.Length &&
                       other.Name == this.Name;
            }

            public override int GetHashCode()
            {
                string str = $"{this.LastWriteTime}{this.Length}{this.Name}";
                return str.GetHashCode();
            }
            public override string ToString()
            {
                return $"{this.Name} {this.Length} {this.LastWriteTime}";
            }
        }
        static void QueryDuplicates2()
        {
            // Change the root drive or folder if necessary.  
            string startFolder = @"c:\program files\Microsoft Visual Studio 9.0\Common7";

            // Make the lines shorter for the console display  
            int charsToSkip = startFolder.Length;

            // Take a snapshot of the file system.  
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(startFolder);
            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            // Note the use of a compound key. Files that match  
            // all three properties belong to the same group.  
            // A named type is used to enable the query to be  
            // passed to another method. Anonymous types can also be used  
            // for composite keys but cannot be passed across method boundaries  
            //
            var queryDupFiles =
                from file in fileList
                group file.FullName.Substring(charsToSkip) by
                    new PortableKey { Name = file.Name, LastWriteTime = file.LastWriteTime, Length = file.Length } into fileGroup
                where fileGroup.Count() > 1
                select fileGroup;

            var list = queryDupFiles.ToList();

            int i = queryDupFiles.Count();

            PageOutput<PortableKey, string>(queryDupFiles);
        }

        // A generic method to page the output of the QueryDuplications methods  
        // Here the type of the group must be specified explicitly. "var" cannot  
        // be used in method signatures. This method does not display more than one  
        // group per page.  
        private static void PageOutput<K, V>(IEnumerable<System.Linq.IGrouping<K, V>> groupByExtList)
        {
            // Flag to break out of paging loop.  
            bool goAgain = true;

            // "3" = 1 line for extension + 1 for "Press any key" + 1 for input cursor.  
            int numLines = Console.WindowHeight - 3;

            // Iterate through the outer collection of groups.  
            foreach (var filegroup in groupByExtList)
            {
                // Start a new extension at the top of a page.  
                int currentLine = 0;

                // Output only as many lines of the current group as will fit in the window.  
                do
                {
                    Console.Clear();
                    Console.WriteLine("Filename = {0}", filegroup.Key.ToString() == String.Empty ? "[none]" : filegroup.Key.ToString());

                    // Get 'numLines' number of items starting at number 'currentLine'.  
                    var resultPage = filegroup.Skip(currentLine).Take(numLines);

                    //Execute the resultPage query  
                    foreach (var fileName in resultPage)
                    {
                        Console.WriteLine("\t{0}", fileName);
                    }

                    // Increment the line counter.  
                    currentLine += numLines;

                    // Give the user a chance to escape.  
                    Console.WriteLine("Press any key to continue or the 'End' key to break...");
                    ConsoleKey key = Console.ReadKey().Key;
                    if (key == ConsoleKey.End)
                    {
                        goAgain = false;
                        break;
                    }
                } while (currentLine < filegroup.Count());

                if (goAgain == false)
                    break;
            }
        }
    }
}
