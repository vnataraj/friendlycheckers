using System;
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
