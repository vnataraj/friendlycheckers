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
    public class UnreachableCodeException : System.Exception {}
    public class CellOutOfBoundsException : System.Exception {} 
    public class InvalidMoveException : System.Exception {}
    public class BadMoveNumberException : System.Exception {}

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
        List<Piece> removals;
        List<Piece> additions;
        
        public Move(int moveNumber, List<Piece> removals, List<Piece> additions) {
            this.moveNumber = moveNumber;
            this.removals = removals;
            this.additions = additions;
        }
        public int getMoveNumber() {
            return moveNumber;
        }

        public List<Piece> getRemovals(){
            return removals;
        }

        public List<Piece> getAdditions() { //does a deep copy of additions
            List<Piece> additionsCopy = new List<Piece>(); 
            foreach (Piece v in this.additions) {
                additionsCopy.Add(new Piece(v));
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
            return new Vector(this.y + move.y, 
                this.x + move.x);
        }
        public Vector divideVector(int divisor) {
            return new Vector(this.getY() / divisor, this.getX() / divisor);
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



    public class Piece { // Piece cannot be changed after created; only copied or read
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

        public Piece newLocation(Vector loc){ 
            return new Piece(this.color, loc, this.type);
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
        //List<Piece> pieces;
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
        public Piece getCellContents(Vector v) {
            return getCellContents(v.getY(), v.getX());
        }
        public Piece getCellContents(int y, int x) {
            if (!(x < width) || !(y < height)) {
                throw new CellOutOfBoundsException();
            }
            return grid[y,x].getPiece(); 
        }

        public void addPieceToCell(Piece piece) {
            grid[piece.getCoordinates().getY(), piece.getCoordinates().getX()].addPiece(piece);
            //pieces.Add(piece);
        }

        public void removePieceFromCell(Piece piece) {
            grid[piece.getCoordinates().getY(), piece.getCoordinates().getX()].removePiece();
            //pieces.Add(piece);
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
                throw new UnreachableCodeException(); 
            }
        }

        public static Vector[] getPossibleJumps(PieceColor color, PieceType type) {
            if (type == PieceType.KING) {
                return kingJumps;
            } else if (type == PieceType.REGULAR) {
                if (color == PieceColor.BLACK) {
                    return blackJumps;
                } else if (color == PieceColor.RED) {
                    return redJumps;
                } else {
                    throw new UnreachableCodeException();
                }
            } else {
                throw new UnreachableCodeException();
            }
        }

        public static Vector[] getPossibleMoves(PieceColor color, PieceType type) {
            if (type == PieceType.KING) {
                return kingMoves;
            } else if (type == PieceType.REGULAR) {
                if (color == PieceColor.BLACK) {
                    return blackMoves;
                } else if (color == PieceColor.RED) {
                    return redMoves;
                } else {
                    throw new UnreachableCodeException();
                }
            } else {
                throw new UnreachableCodeException();
            }
        }

        public GameLogic(int boardWidth, int boardHeight) {
            this.board = new Board(boardWidth, boardHeight);
            moveNumber = 0;
        }

        public bool pollForUpdates(){
            return true; 
            //stub
        }

        public bool isSelectable(int y, int x) {
            return true; // fix this
        }

        public Move makeMove(PieceColor player, int yStart, int xStart, int yEnd, int xEnd) {
            Move myMove = getMove(player, yStart, xStart, yEnd, xEnd);
            doMove(myMove);
            return myMove;
        }

        private void doMove(Move move) {
            if (move.getMoveNumber() == moveNumber + 1) {
                foreach (Piece removal in move.getRemovals()) {
                    board.removePieceFromCell(removal);
                }
                foreach (Piece addition in move.getAdditions()) {
                    board.addPieceToCell(addition);
                }
                moveNumber++;
            } else {
                throw new BadMoveNumberException();
            }
        }

        private void undoMove(Move move) {
            if (move.getMoveNumber() == moveNumber - 1) {
                foreach (Piece addition in move.getAdditions()) { //add removals
                    board.removePieceFromCell(addition);
                }
                foreach (Piece removal in move.getRemovals()) { //remove additions
                    board.addPieceToCell(removal);
                }
                moveNumber--;
            } else {
                throw new BadMoveNumberException();
            }
        }

        private Move getMove(PieceColor player, int yStart, int xStart, int yEnd, int xEnd){
            List<Piece> removals = new List<Piece>();
            List<Piece> additions = new List<Piece>(); 

            Piece start = board.getCellContents(yStart, xStart); 
            Piece end = board.getCellContents(yEnd, yStart);
            Vector endLoc = new Vector(yEnd, yStart); 
  
            if(start == null) { //there is no piece here
                throw new CellEmptyException(); 
            }
            if(end != null) { 
                throw new CellFullException();
            }
            if(start.getColor() != player){ 
                throw new PieceWrongColorException(); 
            }

            Vector myMove = new Vector(yEnd - yStart, xEnd - xStart);
            bool foundValid = false; 

            if(Math.Abs(myMove.getX()) == 1 && Math.Abs(myMove.getY()) == 1) { //move is not a jump
                Vector[] moves = getPossibleMoves(start.getColor(), start.getType());
                foreach(Vector move in moves) { 
                    if (move == myMove) {
                        removals.Add(start);
                        additions.Add(start.newLocation(end.getCoordinates())); 
                        foundValid = true; 
                        break;
                    }
                }
            } else if (Math.Abs(myMove.getX()) == 2 && Math.Abs(myMove.getY()) == 2) { //move is a jump
                Vector[] moves = getPossibleJumps(start.getColor(), start.getType());
                foreach (Vector move in moves) {
                    if (move == myMove) {
                        Vector jumpedLoc = start.getCoordinates().add(move.divideVector(2));
                        Piece jumpedPiece = board.getCellContents(jumpedLoc);
                        if (jumpedPiece == null) {
                            throw new InvalidMoveException();
                        }
                        if (jumpedPiece.getColor() == getOppositeColor(start.getColor())) {
                            removals.Add(start);
                            removals.Add(jumpedPiece);
                            additions.Add(start.newLocation(endLoc));
                            foundValid = true;
                        }
                        break;
                    }
                }
            } else {
                throw new InvalidMoveException();
            }

            if (!foundValid) {
                throw new InvalidMoveException();
            }
            int myMoveNumber = moveNumber + 1; 
            return new Move(myMoveNumber, removals, additions); 
        }
    }
}
