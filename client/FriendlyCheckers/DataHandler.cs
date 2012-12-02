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
            this.username = this.password = "";
            loadCreds();
        }
        public void saveCreds()
        {
            try
            {
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(data_file_name, FileMode.Create, myIsolatedStorage);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(username);
                writer.WriteLine(password);
                writer.Close();
            }
            catch (IsolatedStorageException) { };
        }
        public void loadCreds()
        {
            try
            {
                IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication();
                IsolatedStorageFileStream FS = ISF.OpenFile(data_file_name, FileMode.Open, FileAccess.Read);
                StreamReader SR = new StreamReader(FS);

                Boolean first = true;
                while (!SR.EndOfStream)
                {
                    if (first)
                        username = SR.ReadLine();
                    else
                        password = SR.ReadLine();
                    first = false;
                }
                if (username == null || password == null || username.Equals("") || password.Equals(""))
                    username = password = "";
                SR.Close();
            }
            catch (IsolatedStorageException)
            {
                username = password = "";
            }
        }
        public String getUserName() { return username; }
        public String getPassword() { return password; }
        public void setCreds(String username, String password)
        {
            if (username.Equals("") || password.Equals("")) return;
            this.username = username;
            this.password = password;
            saveCreds();
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
        private PieceColor myColor, whoseMove;
        public SaveData(int matchID, string opponent, int numMoves, PieceColor myColor, PieceColor whoseMove)
        {
            this.matchID = matchID;
            this.opponent = opponent;
            this.numMoves = numMoves;
            this.myColor = myColor;
            this.whoseMove = whoseMove;
        }
        public int getMatchID() { return matchID; }
        public string getOpponent() { return opponent; }
        public int getNumMoves() { return numMoves; }
        public PieceColor getPlayerColor() { return myColor; }
        public PieceColor getWhoseTurn() { return whoseMove; }
    }
}
