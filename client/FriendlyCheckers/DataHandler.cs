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

        public DataHandler()
        {
            this.data_file_name = "fc4.dat";
            this.username = "";
            this.password = "";
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
        public void setCreds(String username, String password)
        {
            if (username.Equals("") || password.Equals("")) return;
            this.username = username;
            this.password = password;
        }
        public Boolean hasCreds()
        {
            return !(username.Equals("") && password.Equals(""));
        }
	}
    public class GameData
    {
        private Move[] moves;
        private PieceColor whoseMove;

        public GameData(Move[] moves, PieceColor whoseMove)
        {
            this.moves = moves;
            this.whoseMove = whoseMove;
        }
        public Move[] getMoves() { return moves; }
        public PieceColor getWhoseMove() { return whoseMove; }
    }
    public class SaveData
    {
        private int matchID;
        private string opponent;
        private int numMoves;

        public SaveData(int matchID, string opponent, int numMoves)
        {
            this.matchID = matchID;
            this.opponent = opponent;
            this.numMoves = numMoves;
        }
        public int getMatchID() { return matchID; }
        public string getOpponent() { return opponent; }
        public int getNumMoves() { return numMoves; }
    }
}
