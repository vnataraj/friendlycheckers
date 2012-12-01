using System;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows;
using System.Collections.Generic;

namespace FriendlyCheckers
{
    public class DataHandler
	{
        private String data_file_name;
        private String username;
        private String password;
        private List<int> saveGameIDs;

        public DataHandler()
        {
            this.data_file_name = "fc4.dat";
            this.username = "";
            this.password = "";

            // Establish a connection to .dat file, and read first line for number of save games
            this.saveGameIDs = new List<int>();
            // for(int i=0; i< line read; i++)
                    //pull save game from database with ID = file.read();
        }
        public void saveGame(int gameID, Board b, PieceColor whoseTurn)
        {   
            saveGame(gameID, b, whoseTurn, "Player 1", "Player 2");
        }
        public void saveGame(int gameID, Board b, PieceColor whoseTurn, String p1, String p2)
        {
            IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(data_file_name, FileMode.Append, myIsolatedStorage);
            StreamWriter writer = new StreamWriter(fileStream);
            writer.Write(gameID);
            writer.Close();

            //
        }
        public void loadGame(int gameID)
        {
            String result = "";
            //this verse is loaded for the first time so fill it from the text file
            IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream FS = ISF.OpenFile(data_file_name, FileMode.Open, FileAccess.Read);
            StreamReader SR = new StreamReader(FS);
            while (!SR.EndOfStream)
                result += SR.ReadLine();
        }
	}
    public class UserGame
    {
        int gameID;
        int matchID;
        string username;

        public UserGame(int gameID, int matchID, string username)
        {
            this.gameID = gameID;
            this.matchID = matchID;
            this.username = username;
        }

        public int getGameID()
        {
            return gameID;
        }

        public int getMatchID()
        {
            return matchID;
        }

        public string getUsername()
        {
            return username;
        }
    }
}
