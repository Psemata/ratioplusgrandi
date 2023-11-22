using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloCostaLopesIA
{
    // Tile states
    public enum TileState
    {
        EMPTY = -1,
        WHITE = 0,
        BLACK = 1
    }
    
    /// <summary>
    /// Node class which represent the node of the "alpha beta" tree
    /// </summary>
    public class Node {
        // Constant used to establish the board size
        const int BOARDSIZE_X = 9;
        const int BOARDSIZE_Y = 7;

        // Both players scores
        int whiteScore = 0;
        int blackScore = 0;

        // Value which tells if the game has ended or not
        public bool GameFinish { get; set; }
           
        // Current node board
        int[,] board;
        Tuple<int, int> move;

        // Children of the current node
        public List<Node> children = new List<Node>();

        // Constructor
        public Node(int[,] board) {
            // Clone the board given in argument
            this.board = board.Clone() as int[,];
        }

        /// <summary>
        /// Evaluate the current state of the game
        /// </summary>
        /// <returns></returns>
        public int Eval(bool white) {
            // It is necessary to always maximize the current player, if the current state of the board is good, the score rise, if not, then the score is lowered.
            // Number of tokens on the board :
            int nToken = GetTokens(white);
            // Stability - the token is stable if it is placed in a position where it can hardly be flipped :
            int stability = GetStability(white);
            // Current mobility - the number of moves possible at the current state
            int cMobility = GetMobility(white);
            // Edge and corner evaluation
            int placement = GetCornersEdges(white);
            // Potential mobility - the number of the opponent's frontier token, which can possibly be a possible position in the future
            int pMobility = GetPotentialMobility(white);
            // If there is at least 5 empty squares, we focus on the gain of tokens
            if(whiteScore + blackScore >= 58) {
                return 15 * nToken; // In the end of the game, we focus on taking the most token possible
            }
            // Otherwise, the other factors are taken into the evaluation
            // At first, it is considered bad to take too much token, so we penalise this
            // We focus on taking strategic points
            return -3 * nToken + 10 * placement + 7 * pMobility + 15 * cMobility + 10 * stability;
        }

        /// <summary>
        /// Evaluate the state of the number of tokens on the board
        /// </summary>
        /// <param name="white"></param>
        /// <returns>tokenState</returns>
        int GetTokens(bool white) {
            // Return value, the state of the current player
            int tokenState;

            // Scores of the two players depending of the turn
            int maximizedPlayerScore;
            int minimizedPlayerScore;

            // Who is to be maximized
            if (white) {
                maximizedPlayerScore = whiteScore;
                minimizedPlayerScore = blackScore;
            } else {
                maximizedPlayerScore = blackScore;
                minimizedPlayerScore = whiteScore;
            }

            if (maximizedPlayerScore >= minimizedPlayerScore) {
                tokenState = maximizedPlayerScore;
            } else {
                tokenState = maximizedPlayerScore - (maximizedPlayerScore + minimizedPlayerScore);
            }

            return tokenState;
        }

        /// <summary>
        /// Return the stability of the specified player's tokens
        /// </summary>
        /// <returns>stability</returns>
        int GetStability(bool white) {
            int[,] stabilityBoard = new int[,] {{ 4,  -3,  2,  2,  2,  2,  2, -3,  4},
                                                {-3,  -4, -1, -1, -1, -1, -1, -4, -3},
                                                { 2,  -1,  1,  0,  1,  0,  1, -1,  2},
                                                { 2,  -1,  0,  1,  0,  1,  0, -1,  2},
                                                { 2,  -1,  1,  0,  1,  0,  1, -1,  2},
                                                {-3,  -4, -1, -1, -1, -1, -1, -4, -3},
                                                { 4,  -3,  2,  2,  2,  2,  2, -3,  4}};

            int stability;

            int maximizedPlayerStability;
            int minimizedPlayerStability;

            int whiteStability = EvaluatePattern(stabilityBoard, true);
            int blackStability = EvaluatePattern(stabilityBoard, false);

            // Who is to be maximized
            if (white) {
                maximizedPlayerStability = whiteStability;
                minimizedPlayerStability = blackStability;
            } else {
                maximizedPlayerStability = blackStability;
                minimizedPlayerStability = whiteStability;
            }

            if (maximizedPlayerStability >= minimizedPlayerStability) {
                stability = maximizedPlayerStability;
            } else {
                stability = maximizedPlayerStability - (maximizedPlayerStability + minimizedPlayerStability);
            }

            return stability;
        }

        /// <summary>
        /// Return the evaluation on the current possible moves of the player
        /// </summary>
        /// <param name="white"></param>
        /// <returns></returns>
        int GetMobility(bool white) {
            int mobility;

            int maximizedPlayerMobility;
            int minimizedPlayerMobility;

            int whiteMobility = GetPossibleMoveIA(true).Count;
            int blackMobility = GetPossibleMoveIA(false).Count;

            // Who is to be maximized
            if (white) {
                maximizedPlayerMobility = whiteMobility;
                minimizedPlayerMobility = blackMobility;
            } else {
                maximizedPlayerMobility = blackMobility;
                minimizedPlayerMobility = whiteMobility;
            }

            if (maximizedPlayerMobility >= minimizedPlayerMobility) {
                mobility = maximizedPlayerMobility;
            } else {
                mobility = maximizedPlayerMobility - (maximizedPlayerMobility + minimizedPlayerMobility);
            }

            return mobility;
        }

        /// <summary>
        /// Get the potential mobility of the current state -> the number of empty spaces sided by the opponents tokens -> possible future places to be
        /// </summary>
        /// <param name="white"></param>
        /// <returns></returns>
        int GetPotentialMobility(bool white) {

            int pMobility;

            int maximizedPlayerPMobility;
            int minimizedPlayerPMobility;

            int sumWhite = 0;
            int sumBlack = 0;

            for (int x = 0; x < BOARDSIZE_X; x++) {
                for (int y = 0; y < BOARDSIZE_Y; y++) {
                    if (board[x, y] == 1) { // Check black tokens --> opponent to the white player
                        sumWhite += CheckEmptySpaces(x, y);
                    } else if (board[x, y] == 0) { // Check white tokens --> opponent to the black player
                        sumBlack += CheckEmptySpaces(x, y);
                    }
                }
            }

            // Who is to be maximized
            if (white) {
                maximizedPlayerPMobility = sumWhite;
                minimizedPlayerPMobility = sumBlack;
            } else {
                maximizedPlayerPMobility = sumBlack;
                minimizedPlayerPMobility = sumWhite;
            }

            if (maximizedPlayerPMobility >= minimizedPlayerPMobility) {
                pMobility = maximizedPlayerPMobility;
            } else {
                pMobility = maximizedPlayerPMobility - (maximizedPlayerPMobility + minimizedPlayerPMobility);
            }

            return pMobility;
        }

        /// <summary>
        /// Check if the token is sided by an empty cell on all the cells except the diagonals
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        int CheckEmptySpaces(int x, int y) {
            int sum = 0;
            if (x-1 >= 0 && board[x - 1,y] == -1) {
                sum += 1;
            }
            if (x+1 < BOARDSIZE_X && board[x + 1, y] == -1) {
                sum += 1;
            }
            if (y - 1 >= 0 && board[x, y - 1] == -1) {
                sum += 1;
            }
            if (y + 1 < BOARDSIZE_Y && board[x, y + 1] == -1) {
                sum += 1;
            }
            return sum;
        }

        /// <summary>
        /// Return the stability of the specified player's tokens
        /// </summary>
        /// <returns>stability</returns>
        int GetCornersEdges(bool white) {
            int[,] cornerEdges = new int[,] {{ 20, -15,   2,   2,   2,   2,   2,  -15,  20},
                                             {-15, -20,   1,   1,   1,   1,   1,  -20, -15},
                                             { 2,    1,   1,   1,   1,   1,   1,   1,    2},
                                             { 2,    1,   1,   1,   1,   1,   1,   1,    2},
                                             { 2,    1,   1,   1,   1,   1,   1,   1,    2},
                                             {-15, -20,   1,   1,   1,   1,   1,  -20, -15},
                                             { 20,  -15,  2,   2,   2,   2,   2,  -15,  20}};

            int cornerAndEdges;

            int maximizedPlayerPlacement;
            int minimizedPlayerPlacement;

            int whitePlacement = EvaluatePattern(cornerEdges, true);
            int blackPlacement = EvaluatePattern(cornerEdges, false);

            // Who is to be maximized
            if (white) {
                maximizedPlayerPlacement = whitePlacement;
                minimizedPlayerPlacement = blackPlacement;
            } else {
                maximizedPlayerPlacement = blackPlacement;
                minimizedPlayerPlacement = whitePlacement;
            }

            if (maximizedPlayerPlacement >= minimizedPlayerPlacement) {
                cornerAndEdges = maximizedPlayerPlacement;
            } else {
                cornerAndEdges = maximizedPlayerPlacement - (maximizedPlayerPlacement + minimizedPlayerPlacement);
            }

            return cornerAndEdges;
        }

        /// <summary>
        /// Apply the operator op on this state and returns the resulting Node
        /// </summary>
        /// <returns></returns>
        public Node Apply(Tuple<int, int> move, bool white) {
            // Create a new child node and give it the current state of the board
            Node child = new Node(this.board);
            // Add it to the children list
            this.children.Add(child);
            // Add the specific move for the given new node
            child.PlayMoveIA(move.Item1, move.Item2, white);
            // Save the move
            this.move = move;
            // Return the new created child
            return child;
        }

        /// <summary>
        /// Evaluate a pattern comparing with the current state of the board
        /// </summary>
        /// <param name="mat_eval"></param>
        /// <param name="white"></param>
        /// <returns></returns>
        private int EvaluatePattern(int[,] boardPattern, bool white) {
            int tokenType = white ? 0 : 1;
            int value = 0;

            for (int x = 0; x < BOARDSIZE_X; x++) {
                for (int y = 0; y < BOARDSIZE_Y; y++) {
                    if (board[x, y] == tokenType) {
                        value += boardPattern[y, x];
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Debugging function which print the current state of the board
        /// </summary>
        private void PrintBoard() {
            for (int y = 0; y < BOARDSIZE_Y; y++) {
                for (int x = 0; x < BOARDSIZE_X; x++) {
                    switch (board[x, y]) {
                        case -1:
                            Console.Write("| " + '*');
                            break;
                        case 1:
                            Console.Write("| " + 'B');
                            break;
                        case 0:
                            Console.Write("| " + 'W');
                            break;
                    }
                }
                Console.WriteLine("|");
            }
            Console.WriteLine();
        }

        #region IA Logic
        /// <summary>
        /// Function used to place a token in the current Node's board
        /// </summary>
        /// <param name="column"></param>
        /// <param name="line"></param>
        /// <param name="isWhite"></param>
        /// <returns>bool which indicate if the move was correctly done</returns>
        public bool PlayMoveIA(int column, int line, bool isWhite) {
            //0. Verify if indices are valid
            if ((column < 0) || (column >= BOARDSIZE_X) || (line < 0) || (line >= BOARDSIZE_Y))
                return false;
            //1. Verify if it is playable
            if (IsPlayableIA(column, line, isWhite) == false)
                return false;

            //2. Create a list of directions {dx,dy,length} where tiles are flipped
            int c = column, l = line;
            bool playable = false;
            TileState opponent = isWhite ? TileState.BLACK : TileState.WHITE;
            TileState ownColor = (!isWhite) ? TileState.BLACK : TileState.WHITE;
            List<Tuple<int, int, int>> catchDirections = new List<Tuple<int, int, int>>();

            for (int dLine = -1; dLine <= 1; dLine++) {
                for (int dCol = -1; dCol <= 1; dCol++) {
                    c = column + dCol;
                    l = line + dLine;
                    if ((c < BOARDSIZE_X) && (c >= 0) && (l < BOARDSIZE_Y) && (l >= 0)
                        && (board[c, l] == (int)opponent))
                    // Verify if there is a friendly tile to "pinch" and return ennemy tiles in this direction
                    {
                        int counter = 0;
                        while (((c + dCol) < BOARDSIZE_X) && (c + dCol >= 0) &&
                                  ((l + dLine) < BOARDSIZE_Y) && ((l + dLine >= 0))
                                   && (board[c, l] == (int)opponent)) // pour éviter les trous
                        {
                            c += dCol;
                            l += dLine;
                            counter++;
                            if (board[c, l] == (int)ownColor) {
                                playable = true;
                                board[column, line] = (int)ownColor;
                                catchDirections.Add(new Tuple<int, int, int>(dCol, dLine, counter));
                            }
                        }
                    }
                }
            }
            // 3. Flip ennemy tiles
            foreach (var v in catchDirections) {
                int counter = 0;
                l = line;
                c = column;
                while (counter++ < v.Item3) {
                    c += v.Item1;
                    l += v.Item2;
                    board[c, l] = (int)ownColor;
                }
            }
            ComputeScoreIA();
            return playable;
        }

        /// <summary>
        /// Overload of the function IsPlayable with a Tuple var -- easier
        /// </summary>
        /// <param name="move"></param>
        /// <param name="isWhite"></param>
        /// <returns></returns>
        public bool IsPlayableIA(Tuple<int, int> move, bool isWhite) {
            return IsPlayableIA(move.Item1, move.Item2, isWhite);
        }

        /// <summary>
        /// Return a boolean which tells if the given position is a playable cell
        /// </summary>
        /// <param name="column"></param>
        /// <param name="line"></param>
        /// <param name="isWhite"></param>
        /// <returns></returns>
        public bool IsPlayableIA(int column, int line, bool isWhite) {
            //1. Verify if the tile is empty !
            if (board[column, line] != (int)TileState.EMPTY)
                return false;
            //2. Verify if at least one adjacent tile has an opponent tile
            TileState opponent = isWhite ? TileState.BLACK : TileState.WHITE;
            TileState ownColor = (!isWhite) ? TileState.BLACK : TileState.WHITE;
            int c = column, l = line;
            bool playable = false;
            List<Tuple<int, int, int>> catchDirections = new List<Tuple<int, int, int>>();
            for (int dLine = -1; dLine <= 1; dLine++) {
                for (int dCol = -1; dCol <= 1; dCol++) {
                    c = column + dCol;
                    l = line + dLine;
                    if ((c < BOARDSIZE_X) && (c >= 0) && (l < BOARDSIZE_Y) && (l >= 0)
                        && (board[c, l] == (int)opponent))
                    // Verify if there is a friendly tile to "pinch" and return ennemy tiles in this direction
                    {
                        int counter = 0;
                        while (((c + dCol) < BOARDSIZE_X) && (c + dCol >= 0) &&
                                  ((l + dLine) < BOARDSIZE_Y) && ((l + dLine >= 0))) {
                            c += dCol;
                            l += dLine;
                            counter++;
                            if (board[c, l] == (int)ownColor) {
                                playable = true;
                                break;
                            } else if (board[c, l] == (int)opponent)
                                continue;
                            else if (board[c, l] == (int)TileState.EMPTY)
                                break;  //empty slot ends the search
                        }
                    }
                }
            }
            return playable;
        }

        /// <summary>
        /// Return a list of all the possible moves
        /// </summary>
        /// <param name="whiteTurn"></param>
        /// <param name="show"></param>
        /// <returns>a list of tuples for all the possible moves</returns>
        public List<Tuple<int, int>> GetPossibleMoveIA(bool whiteTurn, bool show = false) {
            char[] colonnes = "ABCDEFGHIJKL".ToCharArray();
            List<Tuple<int, int>> possibleMoves = new List<Tuple<int, int>>();
            for (int i = 0; i < BOARDSIZE_X; i++)
                for (int j = 0; j < BOARDSIZE_Y; j++) {
                    if (IsPlayableIA(i, j, whiteTurn)) {
                        possibleMoves.Add(new Tuple<int, int>(i, j));
                        //Uncomment if you want to print the possibles moves
                        if (show == true)
                            Console.Write((colonnes[i]).ToString() + (j + 1).ToString() + ", ");
                    }
                }
            return possibleMoves;
        }

        /// <summary>
        /// Get the score of the current situation of the board and tells if there is a winner
        /// </summary>
        private void ComputeScoreIA() {
            whiteScore = 0;
            blackScore = 0;
            foreach (var v in board) {
                if (v == (int)TileState.WHITE)
                    whiteScore++;
                else if (v == (int)TileState.BLACK)
                    blackScore++;
            }
            GameFinish = ((whiteScore == 0) || (blackScore == 0) ||
                        (whiteScore + blackScore == 63));
        }
        #endregion
    }

    public class OthelloBoard : IPlayable.IPlayable
    {
        const int BOARDSIZE_X = 9;
        const int BOARDSIZE_Y = 7;

        int[,] theBoard = new int[BOARDSIZE_X, BOARDSIZE_Y];
        int whiteScore = 0;
        int blackScore = 0;
        public bool GameFinish { get; set; }

        private Random rnd = new Random();

        public OthelloBoard()
        {
            initBoard();
        }

        /// <summary>
        /// Returns the board game as a 2D array of int
        /// with following values
        /// -1: empty
        ///  0: white
        ///  1: black
        /// </summary>
        /// <returns></returns>
        public int[,] GetBoard()
        {
            return (int[,])theBoard;
        }

        #region IPlayable
        public int GetWhiteScore() { return whiteScore; }
        public int GetBlackScore() { return blackScore; }
        public string GetName() { return "Ratio + Nul + Grandi"; }

        /// <summary>
        /// This function is called by the controller to get the next move you want to play
        /// </summary>
        /// <param name="game"></param>
        /// <param name="level"></param>
        /// <param name="whiteTurn"></param>
        /// <returns>The move it will play, will return {-1,-1} if it has to PASS its turn (no move is possible)</returns>
        public Tuple<int, int> GetNextMove(int[,] game, int level, bool whiteTurn)
        {
            List<Tuple<int, int>> possibleMoves = GetPossibleMove(whiteTurn);
            if (possibleMoves.Count == 0)
                return new Tuple<int, int>(-1, -1);
            else
            {
                return AlphaBeta(new Node(theBoard), 5, -1, double.NegativeInfinity, whiteTurn).Item1;
            }
        }

        /// <summary>
        /// Alpha beta function -> the Game IA
        /// </summary>
        public Tuple<Tuple<int,int>,double> AlphaBeta(Node root, int depth, int minOrMax, double parentValue, bool white) {
            // Finishing line
            if(depth == 0 || root.GameFinish)
            {
                return new Tuple<Tuple<int,int>, double>(new Tuple<int, int>(-1, -1), root.Eval(white));
            }
            // Value of the operation
            double optVal = minOrMax * double.NegativeInfinity;
            // Operation
            Tuple<int,int> optOp = null;
            // For all the possible moves, we create a node in the three and evaluate it
            foreach(Tuple<int,int> move in root.GetPossibleMoveIA(white)){
                // Create the new node
                Node child = root.Apply(move, white);
                // AlphaBeta recursion to get its value
                Tuple<Tuple<int,int>, double> result = AlphaBeta(child, (depth - 1), -minOrMax, optVal, !white);
                // Move found to get this value
                Tuple<int,int> dummy = result.Item1;
                // If the value is decent, keep it
                double val = result.Item2;
                if(val * minOrMax >= optVal * minOrMax)
                {
                    optVal = val;
                    optOp = move;
                    if(optVal * minOrMax > parentValue * minOrMax)
                    {
                        break;
                    }
                }
            }
            // Return the best move and its value
            return new Tuple<Tuple<int,int>, double>(optOp, optVal);
        }

        /// <summary>
        /// This function is never called by the controller. 
        /// It is here to help you play on your side
        /// </summary>
        /// <param name="column"></param>
        /// <param name="line"></param>
        /// <param name="isWhite"></param>
        /// <returns></returns>
        public bool PlayMove(int column, int line, bool isWhite)
        {
            //0. Verify if indices are valid
            if ((column < 0) || (column >= BOARDSIZE_X) || (line < 0) || (line >= BOARDSIZE_Y))
                return false;
            //1. Verify if it is playable
            if (IsPlayable(column, line, isWhite) == false)
                return false;
            
            //2. Create a list of directions {dx,dy,length} where tiles are flipped
            int c = column, l = line;
            bool playable = false;
            TileState opponent = isWhite ? TileState.BLACK : TileState.WHITE;
            TileState ownColor = (!isWhite) ? TileState.BLACK : TileState.WHITE;
            List<Tuple<int, int, int>> catchDirections = new List<Tuple<int, int, int>>();

            for (int dLine = -1; dLine <= 1; dLine++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    c = column + dCol;
                    l = line + dLine;
                    if ((c < BOARDSIZE_X) && (c >= 0) && (l < BOARDSIZE_Y) && (l >= 0)
                        && (theBoard[c, l] == (int)opponent))
                    // Verify if there is a friendly tile to "pinch" and return ennemy tiles in this direction
                    {
                        int counter = 0;
                        while (((c + dCol) < BOARDSIZE_X) && (c + dCol >= 0) &&
                                  ((l + dLine) < BOARDSIZE_Y) && ((l + dLine >= 0))
                                   && (theBoard[c, l] == (int)opponent)) // pour éviter les trous
                        {
                            c += dCol;
                            l += dLine;
                            counter++;
                            if (theBoard[c, l] == (int)ownColor)
                            {
                                playable = true;
                                theBoard[column, line] = (int)ownColor;
                                catchDirections.Add(new Tuple<int, int, int>(dCol, dLine, counter));
                            }
                        }
                    }
                }
            }
            // 3. Flip ennemy tiles
            foreach (var v in catchDirections)
            {
                int counter = 0;
                l = line;
                c = column;
                while (counter++ < v.Item3)
                {
                    c += v.Item1;
                    l += v.Item2;
                    theBoard[c, l] = (int)ownColor;
                }
            }
            //Console.WriteLine("CATCH DIRECTIONS:" + catchDirections.Count);
            computeScore();
            return playable;
        }

        /// <summary>
        /// More convenient overload to verify if a move is possible
        /// </summary>
        /// <param name=""></param>
        /// <param name="isWhite"></param>
        /// <returns></returns>
        public bool IsPlayable(Tuple<int, int> move, bool isWhite)
        {
            return IsPlayable(move.Item1, move.Item2, isWhite);
        }

        public bool IsPlayable(int column, int line, bool isWhite)
        {
            //1. Verify if the tile is empty !
            if (theBoard[column, line] != (int)TileState.EMPTY)
                return false;
            //2. Verify if at least one adjacent tile has an opponent tile
            TileState opponent = isWhite ? TileState.BLACK : TileState.WHITE;
            TileState ownColor = (!isWhite) ? TileState.BLACK : TileState.WHITE;
            int c = column, l = line;
            bool playable = false;
            List<Tuple<int, int, int>> catchDirections = new List<Tuple<int, int, int>>();
            for (int dLine = -1; dLine <= 1; dLine++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    c = column + dCol;
                    l = line + dLine;
                    if ((c < BOARDSIZE_X) && (c >= 0) && (l < BOARDSIZE_Y) && (l >= 0)
                        && (theBoard[c, l] == (int)opponent))
                    // Verify if there is a friendly tile to "pinch" and return ennemy tiles in this direction
                    {
                        int counter = 0;
                        while (((c + dCol) < BOARDSIZE_X) && (c + dCol >= 0) &&
                                  ((l + dLine) < BOARDSIZE_Y) && ((l + dLine >= 0)))
                        {
                            c += dCol;
                            l += dLine;
                            counter++;
                            if (theBoard[c, l] == (int)ownColor)
                            {
                                playable = true;
                                break;
                            }
                            else if (theBoard[c, l] == (int)opponent)
                                continue;
                            else if (theBoard[c, l] == (int)TileState.EMPTY)
                                break;  //empty slot ends the search
                        }
                    }
                }
            }
            return playable;
        }
        #endregion

        /// <summary>
        /// Returns all the playable moves in a computer readable way (e.g. "<3, 0>")
        /// </summary>
        /// <param name="v"></param>
        /// <param name="whiteTurn"></param>
        /// <returns></returns>
        public List<Tuple<int, int>> GetPossibleMove(bool whiteTurn, bool show = false)
        {
            char[] colonnes = "ABCDEFGHIJKL".ToCharArray();
            List<Tuple<int, int>> possibleMoves = new List<Tuple<int, int>>();
            for (int i = 0; i < BOARDSIZE_X; i++)
                for (int j = 0; j < BOARDSIZE_Y; j++)
                {
                    if (IsPlayable(i, j, whiteTurn))
                    {
                        possibleMoves.Add(new Tuple<int, int>(i, j));
                        //Uncomment if you want to print the possibles moves
                        //if (show == true)
                        //    Console.Write((colonnes[i]).ToString() + (j + 1).ToString() + ", ");
                    }
                }
            return possibleMoves;
        }

        /// <summary>
        /// Init the board for a new game and save it in the class variable : "theboard"
        /// </summary>
        private void initBoard()
        {
            for (int i = 0; i < BOARDSIZE_X; i++)
                for (int j = 0; j < BOARDSIZE_Y; j++)
                    theBoard[i, j] = (int)TileState.EMPTY;

            theBoard[3, 3] = (int)TileState.WHITE;
            theBoard[4, 4] = (int)TileState.WHITE;
            theBoard[3, 4] = (int)TileState.BLACK;
            theBoard[4, 3] = (int)TileState.BLACK;

            computeScore();
        }

        /// <summary>
        /// Calculate the score, the number of white pawn and black pawn
        /// </summary>
        private void computeScore()
        {
            whiteScore = 0;
            blackScore = 0;
            foreach (var v in theBoard)
            {
                if (v == (int)TileState.WHITE)
                    whiteScore++;
                else if (v == (int)TileState.BLACK)
                    blackScore++;
            }
            GameFinish = ((whiteScore == 0) || (blackScore == 0) ||
                        (whiteScore + blackScore == 63));
        }
    }
}
