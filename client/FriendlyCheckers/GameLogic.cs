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
using System.Runtime.Serialization; 
//using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace FriendlyCheckers {

    public class GameLogicException : System.Exception { }
    public class CellAlreadyFilledException : GameLogicException { }
    public class CellEmptyException : GameLogicException { }
    public class CellFullException : GameLogicException { }
    public class PieceWrongColorException : GameLogicException { }
    public class UnreachableCodeException : GameLogicException { }
    public class CellOutOfBoundsException : GameLogicException { }
    public class InvalidMoveException : GameLogicException { }
    public class PlayerMustJumpException : InvalidMoveException { }
    public class WrongMultiJumpPieceException : InvalidMoveException {} 
    public class BadMoveNumberException : GameLogicException { }
    public class NoMovesLeftException : GameLogicException { }

    public enum PieceColor {RED, BLACK};
    public enum PieceType {REGULAR, KING};
    public enum GameStatus {NOWINNER, REDWINS, BLACKWINS, DRAW};

    //class scheduled for deletion. call GameLogic.getEasyMove, etc. instead.
    public class Player {
        public Player(String name, PieceColor color) {
        }
        public MoveAttempt getEasyMove(GameLogic logic) {
            return logic.getEasyMove(); //stub
        }
        public MoveAttempt getHardMove(GameLogic logic) {
            return logic.getHardMove(); //stub
        }
    }
    public class MoveAttempt {
        int yStart;
        int xStart;
        int yEnd;
        int xEnd;
        public MoveAttempt(int yStart, int xStart, int yEnd, int xEnd) {
            this.yStart = yStart;
            this.xStart = xStart;
            this.yEnd = yEnd;
            this.xEnd = xEnd;
        }
        public int getYStart() {
            return yStart;
        }
        public int getXStart() {
            return xStart;
        }
        public int getYEnd() {
            return yEnd;
        }
        public int getXEnd() {
            return xEnd;
        }
    }

    public class Move { // this is the api to give data to networking (and maybe GUI)
        int turnNumber;
        List<Piece> removals;
        List<Piece> additions;
        PieceColor player; 
        
        public Move(int turnNumber, List<Piece> removals, List<Piece> additions, PieceColor player) {
            this.turnNumber = turnNumber;
            this.removals = removals;
            this.additions = additions;
            this.player = player; 
        }

        public PieceColor getPlayer() { 
            return this.player; 
        }

        public int getTurnNumber() {
            return turnNumber;
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

        public override string ToString() {
            return ":" + getY() + "," + getX() + ":";
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

        public Vector subtract(Vector move) {
            return new Vector(this.y - move.y,
                this.x - move.x);
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

        public Piece newType(PieceType type) {
            return new Piece(this.color, this.coordinates, type);
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

        public int getHeight() {
            return height;
        }
        public int getWidth() {
            return width;
        }

        public static Board deepCopy(Board b) {
            Board newB = new Board(b.getHeight(), b.getWidth());
            for (int y = 0; y < newB.getHeight(); y++) {
                for (int x = 0; x < newB.getWidth(); x++) {
                    Piece p = b.getCellContents(y, x);
                    if (p != null) {
                        newB.addPieceToCell(new Piece(p));
                    }
                }
            }
            return newB;
        }

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
            if (!(x < width) || !(y < height) || !(x >= 0) || !(y >= 0)) {
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
        int moveNumber;
        bool forceJumps;
        int blackPieces = 0;
        int redPieces = 0; 
        int turnNumber = 0; 
        List<Move> movesMade; 

        Vector multiJumpLoc = null; 

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

        public int getMoveNumber() {
            return moveNumber;
        }

        public int getTurnNumber() { 
            return turnNumber; 
        }

        public GameLogic(int boardHeight, int boardWidth) : this 
            (boardHeight, boardWidth, false){}

        public GameLogic(int boardHeight, int boardWidth, bool forceJumps) {
            this.forceJumps = forceJumps; 
            this.board = new Board(boardWidth, boardHeight);
            moveNumber = 0;
            turnNumber = 0;
            movesMade = new List<Move>(); 
        }

        public GameLogic(GameLogic g) { //does a deep copy of GameLogic
            this.forceJumps = g.forceJumps;
            this.board = g.getBoardCopy();
            this.moveNumber = g.moveNumber;
            this.blackPieces = g.blackPieces;
            this.redPieces = g.redPieces;
            if (multiJumpLoc != null) {
                this.multiJumpLoc = new Vector(g.multiJumpLoc);
            } else {
                this.multiJumpLoc = null;
            }
            this.turnNumber = g.turnNumber; 
        }

        public Board getBoardCopy() {
            return Board.deepCopy(board);
        }

        public string getBoardText() {
            string t = "";
            for (int y = 0; y < board.getHeight(); y++) {
                for (int x = 0; x < board.getWidth(); x++) {
                    Piece p = board.getCellContents(y, x);
                    if (p == null) {
                        t += "n";
                    } else {
                        if (p.getColor() == PieceColor.BLACK) {
                            t += "b";
                        } else {
                            t += "r";
                        }
                    }
                }
                t += "\n"; 
            }
            return t;
        }

        public void removePiece(Piece p) {
            if (p.getColor() == PieceColor.BLACK) {
                this.blackPieces--;
            } else {
                this.redPieces--;
            }
            board.removePieceFromCell(p);
        }

        public void addPiece(Piece p){
            if (p.getColor() == PieceColor.BLACK) {
                this.blackPieces++;
            } else {
                this.redPieces++;
            }
            board.addPieceToCell(p); 
        }

        public bool pollForUpdates(){
            return true; 
            //stub
        }

        public bool isSelectable(int y, int x) {
            Piece p = board.getCellContents(y, x); 
            return p.getColor() == this.whoseMove(); 
        }

        public Move makeMove(MoveAttempt a) {
            return makeMove(a.getYStart(), a.getXStart(), a.getYEnd(), a.getXEnd());
        }

        public Move makeMove(int yStart, int xStart, int yEnd, int xEnd) {
            Piece start = board.getCellContents(yStart, xStart);
            Piece end = board.getCellContents(yEnd, xEnd);

            if (start == null) { //there is no piece here
                throw new CellEmptyException();
            }
            if (end != null) {
                throw new CellFullException();
            }
            if (start.getColor() != whoseMove()) {
                throw new PieceWrongColorException();
            }

            PieceType originalPieceType = start.getType(); 

            System.Diagnostics.Debug.WriteLine("makeMove called"); 
            Move myMove = getMove(start, yEnd, xEnd, this.whoseMove());

            doMove(myMove);
            Piece add = myMove.getAdditions()[0]; 
            if (originalPieceType == add.getType() && myMove.getRemovals().Count == 2 && getDoableJumps(add).Count != 0) {
                this.multiJumpLoc = myMove.getAdditions()[0].getCoordinates(); 
                //don't change turnNumber
            } else {
                this.turnNumber++;
                this.multiJumpLoc = null; 
            }
            this.moveNumber++;

            return myMove;
        }

        private void doMove(Move move) {
            movesMade.Add(move); 
            foreach (Piece removal in move.getRemovals()) {
                removePiece(removal); 
            }
            foreach (Piece addition in move.getAdditions()) {
                addPiece(addition);
            }
        }

        public List<Move> getMovesMade() { 
            return this.movesMade; 
        }

        public void makeMoves(List<Move> moves) { 
            PieceColor lastMoveColor = whoseMove(); 
            foreach(Move move in moves) { 
                if(move.getPlayer() != lastMoveColor) { 
                    turnNumber++; 
                    lastMoveColor = move.getPlayer(); 
                }
                moveNumber++; 
                doMove(move); 
            }
        }


#if false
        private void undoMove(Move move) {
            if (move.getMoveNumber() == moveNumber - 1) {
                foreach (Piece addition in move.getAdditions()) { //add removals
                    board.removePieceFromCell(addition);
                }
                foreach (Piece removal in move.getRemovals()) { //remove additions
                    board.addPieceToCell(removal);
                }
                turnNumber--;
            } else {
                throw new BadMoveNumberException();
            }
        } 
#endif

        public PieceColor whoseMove() {
            if (turnNumber % 2 == 1) {
                return PieceColor.RED;
            } else {
                return PieceColor.BLACK;
            }
        }

        public void skipMultiJump() {
            if (this.forceJumps) {
                throw new PlayerMustJumpException();
            } else {
                turnNumber++;
            }
        }

        public Piece givePieceNewLocationKingCheck(Piece currentP, Vector newLoc) { 
            Piece newP = currentP.newLocation(newLoc);
            if (newLoc.getY() == 0 && newP.getColor() == PieceColor.BLACK) {
                newP = newP.newType(PieceType.KING);
            } else if (newLoc.getY() == board.getHeight() - 1 && newP.getColor() == PieceColor.RED) {
                newP = newP.newType(PieceType.KING);
            }
            return newP; 
        }

        public GameStatus getGameStatus() {
            if (!canJumpSomewhere() && !canMoveSomewhere()) {
                if (whoseMove() == PieceColor.BLACK) {
                    if (redPieces > 0) {
                        return GameStatus.REDWINS;
                    }
                } else {
                    if (blackPieces > 0) {
                        return GameStatus.BLACKWINS;
                    }
                }
            }
            if (blackPieces > 0) {
                if (redPieces == 0) {
                    return GameStatus.BLACKWINS;
                }
                return GameStatus.NOWINNER;
            } else if (redPieces > 0) {
                if (blackPieces == 0) {
                    return GameStatus.REDWINS;
                }
                return GameStatus.NOWINNER;
            }
            return GameStatus.DRAW;
        }

        private List<Vector> getDoableMoves(Piece p) {
            Vector[] moves = getPossibleMoves(p.getColor(), p.getType());
            List<Vector> doable = new List<Vector>();

            foreach (Vector move in moves) {
                Vector endLoc = move.add(p.getCoordinates());
                Piece endP;
                try {
                    endP = board.getCellContents(endLoc);
                } catch (CellOutOfBoundsException) {
                    continue;
                }
                if (endP != null) {
                    continue;
                }
                doable.Add(new Vector(move));
            }

            return doable;
        }

        private List<Vector> getDoableJumps(Piece p) { 
            Vector[] jumps = getPossibleJumps(p.getColor(), p.getType());
            List<Vector> doable = new List<Vector>();

            foreach (Vector jump in jumps) {
                Vector endLoc = jump.add(p.getCoordinates());
                Piece endP;
                try {
                    endP = board.getCellContents(endLoc);
                } catch (CellOutOfBoundsException) {
                    continue;
                }
                if (endP != null) {
                    continue;
                }
                Vector middleLoc = p.getCoordinates().add(jump.divideVector(2));
                Piece middleP; 
                try {
                    middleP = board.getCellContents(middleLoc);
                } catch (CellOutOfBoundsException) {
                    continue;
                }
                if (middleP == null) {
                    continue;
                }

                if (middleP.getColor() == p.getColor()) {
                    continue;
                }
                doable.Add(new Vector(jump)); 
            }

            return doable; 
        }

        public MoveAttempt getEasyMove() {
            return getAnyDoableMoveJump(); //stub
        }
        public MoveAttempt getHardMove() {
            return getAnyDoableMoveJump(); //stub
        }

        public MoveAttempt getAnyDoableMoveJump() {
            MoveAttempt jump = getAnyDoableJumpAttempt();
            if (jump != null) {
                return jump;
            }
            MoveAttempt move = getAnyDoableMoveAttempt();
            if (move != null) {
                return move;
            }
            throw new NoMovesLeftException(); 
        }

        public MoveAttempt getAnyDoableMoveAttempt() { 
            PieceColor jumperColor = this.whoseMove();
            for (int y = 0; y < board.getHeight(); y++) {
                for (int x = 0; x < board.getHeight(); x++) {
                    Piece p = board.getCellContents(y, x);
                    if (p == null) {
                        continue;
                    }
                    if (p.getColor() != jumperColor) {
                        continue;
                    }
                    List<Vector> vectors = getDoableMoves(p); 
                    if (vectors.Count > 0) {
                        System.Diagnostics.Debug.WriteLine("can jump somewhere returning moveattempt for" +y+" "+x);
                        return new MoveAttempt(y, x, y+vectors[0].getY(), x+vectors[0].getX());
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("can move somewhere returning false");
            return null;
        }

        private bool canMoveSomewhere() {
            return getAnyDoableMoveAttempt() != null;
        }

        public MoveAttempt getAnyDoableJumpAttempt() {
            PieceColor jumperColor = this.whoseMove();
            for (int y = 0; y < board.getHeight(); y++) {
                for (int x = 0; x < board.getHeight(); x++) {
                    Piece p = board.getCellContents(y, x);
                    if (p == null) {
                        continue;
                    }
                    if (p.getColor() != jumperColor) {
                        continue;
                    }
                    List<Vector> doable = getDoableJumps(p);
                    if (doable.Count > 0) {
                        System.Diagnostics.Debug.WriteLine("can jump somewhere returning true. " + y + " " + x);
                        return new MoveAttempt(y, x, y + doable[0].getY(), x + doable[0].getX()); 
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("can jump somewhere returning false");
            return null;
        }

        private bool canJumpSomewhere() {
            return getAnyDoableJumpAttempt() != null; 
        }

        private bool moveIsJump(Vector start, int yEnd, int xEnd) {
            return (Math.Abs(start.getX() - xEnd) == 2 && Math.Abs(start.getY() - yEnd) == 2);
        }

        private Move getMove(Piece start, int yEnd, int xEnd, PieceColor player){
            List<Piece> removals = new List<Piece>();
            List<Piece> additions = new List<Piece>(); 

            if(multiJumpLoc != null) { 
                if(multiJumpLoc.Equals(start.getCoordinates()) && moveIsJump(start.getCoordinates(), yEnd, xEnd)) { 
                } else {
                    throw new WrongMultiJumpPieceException();
                }
            }

            System.Diagnostics.Debug.WriteLine("start vector is " + start.getCoordinates().ToString());

            Vector startLoc = start.getCoordinates();
            Vector endLoc = new Vector(yEnd, xEnd);  
  


            Vector myMove = endLoc.subtract(startLoc);

            //jump logic goes here
            if (this.forceJumps && canJumpSomewhere()) {
                List<Vector> jumps = getDoableJumps(start);
                bool found = false;
                foreach (Vector jump in jumps) {
                    if (jump.Equals(myMove)) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    throw new PlayerMustJumpException();
                }
            }

            
            System.Diagnostics.Debug.WriteLine("myMove is " + myMove.ToString()); 
            bool foundValid = false; 

            if(Math.Abs(myMove.getX()) == 1 && Math.Abs(myMove.getY()) == 1) { //move is not a jump
                System.Diagnostics.Debug.WriteLine("Move is not a jump.");
                Vector[] moves = getPossibleMoves(start.getColor(), start.getType());
                foreach(Vector move in moves) {
                    System.Diagnostics.Debug.WriteLine("testing possible move " + move.ToString()); 
                    if (myMove.Equals(move)) {
                        removals.Add(start);
                        additions.Add(givePieceNewLocationKingCheck(start, start.getCoordinates().add(myMove))); 
                        foundValid = true; 
                        break;
                    }
                }
            } else if (Math.Abs(myMove.getX()) == 2 && Math.Abs(myMove.getY()) == 2) { //move is a jump
                Vector[] moves = getPossibleJumps(start.getColor(), start.getType());
                foreach (Vector move in moves) {
                    if (myMove.Equals(move)) {
                        Vector jumpedLoc = start.getCoordinates().add(move.divideVector(2));
                        Piece jumpedPiece = board.getCellContents(jumpedLoc);
                        if (jumpedPiece == null) {
                            System.Diagnostics.Debug.WriteLine("cannot jump an empty square");
                            throw new InvalidMoveException();
                        }
                        if (jumpedPiece.getColor() == getOppositeColor(start.getColor())) {
                            removals.Add(start);
                            removals.Add(jumpedPiece);
                            additions.Add(givePieceNewLocationKingCheck(start, endLoc));
                            foundValid = true;
                        } else {
                            System.Diagnostics.Debug.WriteLine("cannot jump your own piece");
                            throw new InvalidMoveException();
                        }
                        break;
                    }
                }
            } else {
                System.Diagnostics.Debug.WriteLine("vector is wrong length"); 
                throw new InvalidMoveException();
            }

            if (!foundValid) {
                System.Diagnostics.Debug.WriteLine("Could not find match vector");
                throw new InvalidMoveException();
            }
            int myTurnNumber = turnNumber + 1; 
            return new Move(myTurnNumber, removals, additions, player); 
        }
    }
}
