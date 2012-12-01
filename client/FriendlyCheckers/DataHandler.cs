using System;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows;

namespace FriendlyCheckers
{
    public class DataHandler
	{
        private String data_file_name;
        private String username;
        private String password;
        private int[] saveGameIDs;

        public DataHandler()
        {
            this.data_file_name = "fc4.dat";
            this.username = "";
            this.password = "";

            // Establish a connection to .dat file, and read first line for number of save games
            this.saveGameIDs = new int[0/*this will later be the size;*/];
            IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(data_file_name, FileMode.Create, myIsolatedStorage);
            StreamWriter writer = new StreamWriter(fileStream);
            writer.Write("Username");
            writer.Write("3");
            writer.Write("41627");
            writer.Write("13478");
            writer.Write("99200");
            writer.Close();


            String result = "";
            //this verse is loaded for the first time so fill it from the text file
            IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream FS = ISF.OpenFile(data_file_name, FileMode.Open, FileAccess.Read);
            StreamReader SR = new StreamReader(FS);
            while(!SR.EndOfStream)
                result += SR.ReadLine();
            MessageBox.Show("Result = " + result);

            // for(int i=0; i< line read; i++)
                    //pull save game from database with ID = file.read();
        }
	}
}
