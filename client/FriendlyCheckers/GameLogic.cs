using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace FriendlyCheckers {


    public class CellAlreadyFilledException : System.Exception {}
    public class CellEmptyException : System.Exception {}
    public class CellFullException : System.Exception {}
    public class PieceWrongColorException : System.Exception {}

    public enum PieceColor {RED, BLACK};
    public enum PieceType {REGULAR, KING};



    
    public class UserGame { 
        int gameID; 
        int matchID; 
        string username; 

        public UserGame(int gameID, int matchID, string username) { 
            this.gameID = gameID; 
            this.matchID = matchID; 
            this.username = username; 
        }

        public int getGameID(){ 
            return gameID; 
        } 

        public int getMatchID(){ 
            return matchID;
        }

        public string getUsername(){
            return username; 
        }
    }

    public class Move { // this is the api to give data to networking (and maybe GUI)
        int moveNumber;
        List<Vector> removals;
        List<Vector> additions;
        
        public Move(int moveNumber, List<Vector> removals, List<Vector> additions) {
            this.moveNumber = moveNumber;
            this.removals = removals;
            this.additions = additions;
        }
        public int getMoveNumber() {
            return moveNumber;
        }

        public List<Vector> getRemovals(){ //does a deep copy of removals
            List<Vector> removalsCopy = new List<Vector>(); 
            foreach(Vector v in this.removals) {
                removalsCopy.Add(new Vector(v)); 
            }
            return removalsCopy;
        }

        public List<Vector> getAdditions() { //does a deep copy of additions
            List<Vector> additionsCopy = new List<Vector>(); 
            foreach (Vector v in this.additions) {
                additionsCopy.Add(new Vector(v));
            }
            return additionsCopy;
        }


    }

    public class Vector {
        private int x;
        private int y; 

        // Constructor: 
        public Vector(int y, int x) {
            this.x = x;
            this.y = y;
        }

        public int getX() { 
            return x; 
        }

        public int getY(){ 
            return y; 
        }

        //copy Constructor: 
        public Vector(Vector copyable) { 
            this.x = copyable.getX(); 
            this.y = copyable.getY(); 
        }

        public Vector add(Vector move) {
            return new Vector(this.x + move.x, 
                this.y + move.y);
        }

        public override bool Equals(Object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;
            Vector v = (Vector)obj;
            return this.x == v.getX() && this.y == v.getY();
        }
        public override int GetHashCode() {
            return x ^ y;
        }
    }



    public class Piece {
        PieceColor color;
        Vector coordinates;
        PieceType type;

        public Piece(PieceColor color, Vector coordinates, PieceType type) {
            this.color = color;
            this.coordinates = coordinates;
            this.type = type;
        }
        public Piece(Piece copyable) {
            this.color = copyable.color;
            this.coordinates = copyable.coordinates;
            this.type = copyable.type;
        }
        public PieceType getType(){ 
            return type; 
        }
        public PieceColor getColor(){ 
            return color; 
        }
        public Vector getCoordinates(){ 
            return new Vector(coordinates); 
        }
    }


    public class Cell { 
        bool filled; 
        Piece piece; 
        
        public Cell() { 
            this.filled = false; 
            this.piece = null; 
        }

        public void addPiece(Piece piece) { 
            if(this.filled) { 
                throw new CellAlreadyFilledException(); 
            } else { 
                this.filled = true; 
            }
            this.piece = piece; 
        }

        public Piece removePiece() { 
            if( ! this.filled) { 
                throw new CellEmptyException(); 
            } else { 
                this.filled = false; 
            }
            Piece temp = this.piece;
            this.piece = null; 
            return temp; 
        }

        public Piece getPiece() { 
            if( ! this.filled) { 
                return null; 
            }
            return new Piece(this.piece); 
        }
    }



    public class Board {
        List<Piece> pieces;
        Cell[,] grid;
        int height;
        int width;

        public Board(int height, int width) {
            this.grid = new Cell[height, width];
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    this.grid[y, x] = new Cell();
                }
            }
            this.height = height;
            this.width = width;
        }

        public Piece getCellContents(int y, int x) {
            if (!(x < width) || !(y < height)) {
                throw new Exception();
            }
            return grid[y,x].getPiece(); 
        }

        public void addPieceToCell(int y, int x, Piece piece) {
            Piece newPiece = new Piece(piece);
            grid[y, x].addPiece(newPiece);
            pieces.Add(piece);
        }
    }

    public class GameLogic {
        Board board; 
        PieceColor whoseTurn;
        int moveNumber;

        static Vector [] kingMoves = new Vector[4]{new Vector(1,1), new Vector(1,-1), new Vector(-1,-1), new Vector(-1,1)}; // kings move anywhere
        static Vector [] blackMoves = new Vector[2]{new Vector(-1,1), new Vector(-1,-1)}; // black moves up
        static Vector [] redMoves = new Vector[2]{new Vector(1,1), new Vector(1,-1)}; // red moves down

        static Vector [] kingJumps = new Vector[4]{new Vector(2,2), new Vector(2,-2), new Vector(-2,-2), new Vector(-2,2)}; // kings move anywhere
        static Vector [] blackJumps = new Vector[2]{new Vector(-2,2), new Vector(-2,-2)}; // black moves up
        static Vector [] redJumps = new Vector[2]{new Vector(2,2), new Vector(2,-2)}; // red moves down

        public static PieceColor getOppositeColor(PieceColor color){ 
            if (color == PieceColor.BLACK) { 
                return PieceColor.RED; 
            } else if(color == PieceColor.RED) { 
                return PieceColor.BLACK; 
            } else { 
                throw new Exception("what else could it be?"); 
            }
        }

        public static Vector[] getPossibleMoves(PieceColor color, PieceType type) {
            return kingJumps;
        }

        public GameLogic(int boardWidth, int boardHeight) {
            this.board = new Board(boardWidth, boardHeight);
            moveNumber = 0;
        }

        public bool pollForUpdates(){
            return true; 
            //stub
        }



        public Move makeMove(PieceColor player, int yStart, int xStart, int yEnd, int xEnd){
            List<Vector> removals = new List<Vector>();
            List<Vector> additions = new List<Vector>(); 

            Piece start = board.getCellContents(yStart, xStart); 
            Piece end = board.getCellContents(yEnd, yStart);
  
            if(start == null) { //there is no piece here
                throw new CellEmptyException(); 
            }
            if(end != null) { 
                throw new CellFullException();
            }
            if(start.getColor() != player){ 
                throw new PieceWrongColorException(); 
            }
            Vector movement = new Vector(yEnd - yStart, xEnd - xStart);

            Vector[] moves = getPossibleMoves(start.getColor(), start.getType());




            moveNumber++; 
            return new Move(moveNumber, removals, additions); 
            

        }


            


    }
}
