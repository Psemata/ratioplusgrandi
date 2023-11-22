using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Othello.Annotations;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

using ControllerOthello;

namespace Othello
{
    /// <summary>
    /// Interaction de la GUI
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region FIELDS_PROPERTIES
        private int playerTurn = 0;
        public const int PASS = -1;
        private int delay = 100;

        private const int BOARDSIZE_X = 9;
        private const int BOARDSIZE_Y = 7;

        //ObservableCollection IAcollection = new ObservableCollection();
        IPlayable.IPlayable Referee = null;
        IPlayable.IPlayable tournamentBlackPlayer = null;
        IPlayable.IPlayable tournamentWhitePlayer = null;

        private LogicBoard logicBoard;
        private readonly SolidColorBrush playableColor = new SolidColorBrush(Color.FromRgb(0, 111, 111));
        private readonly SolidColorBrush regularColor = new SolidColorBrush(Colors.Green);
        private System.Collections.ObjectModel.ObservableCollection<IPlayable.IPlayable> tournamentPlayers = new System.Collections.ObjectModel.ObservableCollection<IPlayable.IPlayable>();

        public int BlackScore => logicBoard.GetBlackScore();

        public int WhiteScore => logicBoard.GetWhiteScore();

        public string BlackTimeLeft => $"{blackPlayer.MinutesLeft} : {blackPlayer.SecondsLeft}";

        public string WhiteTimeLeft => $"{whitePlayer.MinutesLeft} : {whitePlayer.SecondsLeft}";

        public string CurrentTurn => $"Player {(playerTurn%2 == 0 ? "black" : "white")} turn";
        
        private List<Ellipse> pawns = new List<Ellipse>();
        
        public Player whitePlayer = new Player("WhitePlayer", Pawn.Colors.White);

        public Player blackPlayer = new Player("BlackPlayer", Pawn.Colors.Black);

        DispatcherTimer mainTimer;

        private System.IO.Stream stream;
        public Save save;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            lbIABlack.ItemsSource = tournamentPlayers;
            lbIAWhite.ItemsSource = tournamentPlayers;
            mainTimer = new DispatcherTimer();
            mainTimer.Interval = new TimeSpan(0,0,0,0,50); // chaque 100ms
            // Chargement du board
            logicBoard = new LogicBoard();
            logicBoard.fillBoard();
            loadFromLogicBoard();
            DataContext = this;
            mainTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 1)};

            //Timer principal pour les 2 horloges
            mainTimer.Tick += (sender, args) =>
            {
                whitePlayer.tick();
                blackPlayer.tick();

                NotifyPropertyChanged("WhiteTimeLeft");
                NotifyPropertyChanged("BlackTimeLeft");

               if(IsGameFinished())
                {
                    NotifyPropertyChanged("BlackScore");
                    NotifyPropertyChanged("WhiteScore");
                    NotifyPropertyChanged("CurrentTurn");
                    EndGame();
                }
            };
            mainTimer.Start();
            RestartGame();
        }
        
        /// <summary>
        /// Event appeler lors du click de la souris sur le board.
        /// Cette méthode check si le coup est jouable et l'applique.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)e.Source;
            int c = Grid.GetColumn(element);
            int r = Grid.GetRow(element);
            PlayMove(c, r);
         }

        /// <summary>
        /// Updates the board visually according to the logicBoard state
        /// </summary>
        /// <param name="c">The column number of thhe move (0..7)</param>
        /// <param name="r">The row number of the move (0..7)</param>
        private void PlayMove(int c, int r)
        {
            var playable = logicBoard.IsPlayable(c, r, playerTurn % 2 != 0);

            if (playable)
            {
                logicBoard.PlayMove(c, r, playerTurn % 2 != 0);
                loadFromLogicBoard();

                if (playerTurn % 2 == 0){
                    whitePlayer.start();
                    blackPlayer.stop();
                }
                else{
                    blackPlayer.start();
                    whitePlayer.stop();
                }
                playerTurn++;
                if (!CanPlayerPlay(playerTurn) && !IsGameFinished())
                {
                    playerTurn++;
                    string color = (playerTurn % 2 != 0) ? "white" : "black";
                    Console.WriteLine($"{color} cannot play this turn");
                }

                Console.WriteLine($" player turn is {playerTurn}");
                NotifyPropertyChanged("BlackScore");
                NotifyPropertyChanged("WhiteScore");
                NotifyPropertyChanged("CurrentTurn");
            }

        }

        /// <summary>
        /// Met à jours le board GUI avec le board Logic
        /// </summary>
        private void loadFromLogicBoard()
        {
            pawns.ForEach(p => othelloBoard.Children.Remove(p));
            pawns.Clear();

            for (var i = 0; i < LogicBoard.HEIGHT; i++)
            {
                for (var j = 0; j < LogicBoard.WIDTH; j++)
                {
                    var value = logicBoard.Board[i, j];

                    if (value != null)
                    {
                        var ellipse = new Ellipse
                        {
                            Height = 40,
                            Width = 40,
                            Stroke = Brushes.Black,
                            Fill = value.Color == Pawn.Colors.White ? Brushes.White : Brushes.Black
                        };
                        pawns.Add(ellipse);

                        Grid.SetColumn(ellipse, j);
                        Grid.SetRow(ellipse, i);
                    }
                }
            }
            pawns.ForEach(p => othelloBoard.Children.Add(p));
        }

        private void menuExitClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// Check lors du survol de la souris si le coup est jouable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBoardMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var source = e.Source as Rectangle;

            if (source == null) return;

            var rectHover = source;
            var x = Grid.GetColumn(rectHover);
            var y = Grid.GetRow(rectHover);

            var playable = logicBoard.IsPlayable(x, y, playerTurn % 2 != 0);

            othelloBoard.Children.Cast<UIElement>()
                .ToList()
                .Where(c => c is Rectangle)
                .Cast<Rectangle>()
                .ToList()
                .ForEach(c => c.Fill = c.Equals(rectHover) && playable ? playableColor : regularColor);    
        }
        
        /// <summary>
        /// Check si pour le joueur spécifié il reste un coup jouable
        /// </summary>
        /// <param name="playerTurn"></param>
        /// <returns></returns>
        public bool CanPlayerPlay(int playerTurn)
        {
            return othelloBoard.Children
                .Cast<UIElement>()
                .ToList()
                .Where(c => c is Rectangle)
                .Cast<Rectangle>()
                .ToList() 
                .Where(c => logicBoard.Board[Grid.GetRow(c), Grid.GetColumn(c)] == null)// Are there opened gaps ?
                .Any(c => logicBoard.IsPlayable(Grid.GetColumn(c), Grid.GetRow(c), playerTurn%2 != 0)); //if yes, are they playable ?
        }

        /// <summary>
        /// Termine le jeu et demande si le joueur souhaite rejouer
        /// </summary>
        public void EndGame()
        {
            var winner = logicBoard.GetWhiteScore() < logicBoard.GetBlackScore() ? "Black" : "White";
            System.Windows.MessageBox.Show($"{winner} has won !", "End of game");


            mainTimer.Stop();

            if (System.Windows.MessageBox.Show($"{winner} has won !\n Replay ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {

                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                RestartGame();
            }
        }

        /// <summary>
        /// Redémarre le jeu
        /// </summary>
        private void RestartGame()
        {
            logicBoard.fillBoard();
            loadFromLogicBoard();
            whitePlayer.reset();
            blackPlayer.reset();
            playerTurn = 0;
            mainTimer.Start();
        }

        /// <summary>
        /// Est-ce le jeu est est terminé ?
        /// </summary>
        /// <returns></returns>
        private bool IsGameFinished()
        {
            return (!CanPlayerPlay(playerTurn) && !CanPlayerPlay(playerTurn + 1))
                //|| logicBoard.Board.Cast<Pawn>().ToList().Count(p => p == null) == 0 
                 || whitePlayer.TotalSecondsLeft <= 0 || blackPlayer.TotalSecondsLeft <=0 ;
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region SERIALIZATION

        /// <summary>
        /// Sauvegarde de la partie courante (Sérialisation)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveClick(object sender, RoutedEventArgs e)
        {

            if(playerTurn%2 == 0)
            {
                //black is current player
                save = new Save(logicBoard, blackPlayer, whitePlayer, playerTurn);
            }
            else
            {
                save = new Save(logicBoard, whitePlayer, blackPlayer, playerTurn);
            }

            SaveFileDialog Filedialog = new SaveFileDialog();
            Filedialog.DefaultExt = ".xml";
            Filedialog.Filter = "XML documents (.xml)|*.xml";
            System.Windows.Forms.DialogResult result = Filedialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = Filedialog.FileName;
                stream = System.IO.File.Open(fileName, System.IO.FileMode.Create);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                bformatter.Serialize(stream, save);
                stream.Close();

                System.Windows.MessageBox.Show("Sauvegarde réussie");
            }
            else
            {
                System.Windows.MessageBox.Show("Sauvegarde échouée, aucun fichier n'a été choisi");
            }

            
        }

        /// <summary>
        /// Chargement d'une partie sérlialisée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadClick(object sender, RoutedEventArgs e)
        {
            save = null;
            OpenFileDialog dgl = new OpenFileDialog();
            dgl.DefaultExt = ".xml";
            dgl.Filter = "XML Documents (.xml)|*.xml";

            DialogResult result = dgl.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                string file = dgl.FileName;

                stream = System.IO.File.Open(file, System.IO.FileMode.Open);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                save = (Save)bformatter.Deserialize(stream);
                stream.Close();

                logicBoard = save.Board;
                playerTurn = save.turn;

                if (playerTurn % 2 == 0)
                {
                    blackPlayer = save.CurrentPlayer;
                    whitePlayer = save.Player2;
                }
                else
                {
                    whitePlayer = save.CurrentPlayer;
                    blackPlayer = save.Player2;
                }

                System.Windows.MessageBox.Show("Chargement réussi");

                loadFromLogicBoard();
                NotifyPropertyChanged("BlackScore");
                NotifyPropertyChanged("WhiteScore");
                NotifyPropertyChanged("CurrentTurn");
            }
            else
            {
                System.Windows.MessageBox.Show("Chargement échoué, pas de fichier spécifié");
            }
        }

        #endregion

        private void MenuResetGame_Click(object sender, RoutedEventArgs e)
        {
            RestartGame();
        }

        #region GAMELOOP

        /// <summary>
        /// verify if both boards given as 2D int arrays are equal
        /// </summary>
        /// <param name="b1">First player board game status</param>
        /// <param name="b2">Second player board game status</param>
        /// <returns>Boolean comparison result (what did you think?)</returns>
        public static bool boardCompare(int[,] b1, int[,] b2)
        {
            for (int i = 0; i < BOARDSIZE_X ; i++)
            {
                for (int j = 0; j < BOARDSIZE_Y; j++)
                {
                    if (b1[i, j] != b2[i, j])
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// This method will perform the game loop 
        /// NOTE: it is required for this method to be async to allow for await
        /// otherwise it will lock the main(UI) thread and the game status is not
        /// visually updated (refreshed)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuStartGame_Click(object sender, RoutedEventArgs e)
        {
            if ((tournamentBlackPlayer == null)||(tournamentWhitePlayer == null))
            {
                System.Windows.MessageBox.Show("You NEED 2 IAs to play a match!");
                return;
            }
            // to prevent crash with some IA when playing a second time
            tournamentBlackPlayer = (IPlayable.IPlayable)Activator.CreateInstance(tournamentPlayers[lbIABlack.SelectedIndex].GetType());
            tournamentWhitePlayer = (IPlayable.IPlayable)Activator.CreateInstance(tournamentPlayers[lbIAWhite.SelectedIndex].GetType());

            RestartGame();
            Referee = new ControllerOthello.Board();

            int[,] refBoard = (Referee as ControllerOthello.Board).GetBoard();
            int[,] board1 = refBoard;   //tournamentBlackPlayer.GetBoard();   // PLAYER 1
            int[,] board2 = refBoard;   //tournamentWhitePlayer.GetBoard();   // PLAYER 2
            team1.Foreground = new SolidColorBrush(Color.FromRgb(0,0,0));
            team2.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            Tuple<int, int> playerMove = null;
            int passCount = 0;
            bool testPlayer1, testPlayer2;
            bool whitePlays = false;
            IPlayable.IPlayable activePlayer = tournamentBlackPlayer; // player 1 begins playing black
            
            //GAME LOOP
            while (boardCompare(refBoard, board1) && boardCompare(refBoard, board2) && (passCount < 2))
            {
                int totalScore = Referee.GetBlackScore() + Referee.GetWhiteScore();
                await Task.Delay(whitePlays? 100 : 100); // slow down game speed
                try
                {
                     playerMove = activePlayer.GetNextMove(refBoard, 5, whitePlays);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace);
                }
                // check move validity
                if (playerMove == null)
                {
                    System.Windows.MessageBox.Show($" {activePlayer.GetName()} a renvoyé un coup NULL\nFIN DE LA PARTIE");
                    return;
                }
                if ((playerMove.Item1 == PASS) && (playerMove.Item2 == PASS))
                {
                    Console.WriteLine("PASS");
                    passCount++;
                    ///TODO verify if no move was possible (no playable move otherwise display a msg or throw something
                    if ((Referee as ControllerOthello.Board).GetPossibleMoves(whitePlays).Count != 0)
                    {
                        System.Windows.MessageBox.Show($" {activePlayer.GetName()} a passé son tour de manière illégale\nFIN DE LA PARTIE");
                        return;

                    }
                }
                else
                {
                    passCount = 0;
                    Console.WriteLine($"{activePlayer.GetName()}: {(char)('A' + playerMove.Item1)}{1 + playerMove.Item2}");
                    // check validity
                    if ((Referee as ControllerOthello.Board).IsPlayable(playerMove, whitePlays))
                    {
                        Console.WriteLine($"Coup valide de {activePlayer}");
                        // play the move for both players and referee
                        Referee.PlayMove(playerMove.Item1, playerMove.Item2, whitePlays);
                        testPlayer1 = tournamentBlackPlayer.PlayMove(playerMove.Item1, playerMove.Item2, whitePlays);  // no verification here
                        testPlayer1 = tournamentWhitePlayer.PlayMove(playerMove.Item1, playerMove.Item2, whitePlays);  // no verification here

                        // compare boards for validity
                        refBoard = (Referee as ControllerOthello.Board).GetBoard();
                        board1 = tournamentBlackPlayer.GetBoard();
                        board2 = tournamentWhitePlayer.GetBoard();
                        testPlayer1 = boardCompare(refBoard, board1);
                        testPlayer2 = boardCompare(refBoard, board2);
                        if (testPlayer1 && testPlayer2)  // the move is valid
                        {
                            PlayMove(playerMove.Item1, playerMove.Item2);
                            UI.Title = $"BLACK:{Referee.GetBlackScore()} WHITE:{Referee.GetWhiteScore()}";
                            //System.Windows.MessageBox.Show(".");       
                            await Task.Delay(delay);
                        }
                        else if (!testPlayer1)
                        {
                            System.Windows.MessageBox.Show($"Board state of {tournamentBlackPlayer.GetName()}is incorrect");
                            throw new Exception($"Board state of {tournamentBlackPlayer.GetName()}is incorrect");
                        }
                        else if (!testPlayer2)
                        {
                            System.Windows.MessageBox.Show($"Board state of {tournamentWhitePlayer.GetName()}is incorrect");
                            throw new Exception($"Board state of {tournamentWhitePlayer.GetName()} is incorrect");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"COUP INVALIDE {(char)(playerMove.Item1+'A')}{playerMove.Item2+1}");
                        if (whitePlays == true)
                        {
                            team2.Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100));
                            System.Windows.MessageBox.Show($"COUP INVALIDE de {tournamentWhitePlayer.GetName()} {(char)playerMove.Item1 + 'A'}{playerMove.Item2 + 1}\nFIN DE LA PARTIE");
                            return;
                        }
                        else
                        {
                            team1.Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100));
                            System.Windows.MessageBox.Show($"COUP INVALIDE de {tournamentBlackPlayer.GetName()} {(char)(playerMove.Item1 + 'A')}{playerMove.Item2 + 1}\nFIN DE LA PARTIE");
                            return;
                        }
                    }
                }
                // SWAP players and color
                whitePlays = !whitePlays;
                activePlayer = (activePlayer == tournamentBlackPlayer) ? tournamentWhitePlayer : tournamentBlackPlayer;
            } // end of GAMELOOP
        }
        #endregion

        /// <summary>
        /// Will look for all DLL in the current folder that implement IPlayable and have a Board class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuFindIA_Click(object sender, RoutedEventArgs e)
        {
            tournamentPlayers.Clear();
            //find all DLLs with name starting with "OthelloIA" in the executable folder
            List<Assembly> IAPlayers = new List<Assembly>();
            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (string dll in Directory.GetFiles(path, "Othello*.dll"))
                IAPlayers.Add(Assembly.LoadFile(dll));
            foreach (Assembly assembly in IAPlayers)
            {
                try
                {
                    Assembly.Load(assembly.FullName);
                }
                catch
                {
                    System.Windows.MessageBox.Show("Impossible de charger " + assembly.FullName);
                }
                // verify if they implement IPlayable and have a Board class
                IPlayable.IPlayable player = null;
                Type[] types = assembly.GetTypes();
                for (int i = 0; i < types.Count(); i++)
                {
                    if (types[i].Name.Contains("Board"))       // the IA's class that implements IPlayable must have "Board" in its name. E.g OthelloBoard, TheBoard, MyBoard, ...
                        player = (IPlayable.IPlayable)Activator.CreateInstance(types[i]);  // requires a default constructore
                }
                if (player != null) // add it to the players list
                {
                    tournamentPlayers.Add(player);
                }
            }
         }

        #region HANDLERS
        // will select as Team 1 with left click or  Team2 with rightt click
        private void Btn_ClickLeft(object sender, MouseButtonEventArgs e)
        {
            System.Windows.MessageBox.Show("Youhou");
        }

        private void lbIABlack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                tournamentBlackPlayer = (IPlayable.IPlayable)Activator.CreateInstance(tournamentPlayers[lbIABlack.SelectedIndex].GetType());
                team1.Content = tournamentBlackPlayer.GetName();
            }
            catch
            {
                System.Windows.MessageBox.Show("Problème à l'instanciation de la classe IA (Black)");
            }
        }

        private void lbIAWhite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                tournamentWhitePlayer = (IPlayable.IPlayable)Activator.CreateInstance(tournamentPlayers[lbIAWhite.SelectedIndex].GetType());
                team2.Content = tournamentWhitePlayer.GetName();
            }
            catch
            {
                System.Windows.MessageBox.Show("Problème à l'instanciation de la classe IA (White)");
            }
        }
        #endregion


        /// <summary>
        /// Method to test the IA is complying with Othello rules
        /// BROKEN (does not work)
        /// Probably that putting random pawns all over the board is not a good way to 
        /// initialize a playable board
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuTestIA_Click(object sender, RoutedEventArgs e)
        {
            Referee = new ControllerOthello.Board();
            if ((lbIABlack.SelectedIndex ==-1) || (tournamentPlayers.Count ==0))
                return; 
            IPlayable.IPlayable testBlackPlayer = (IPlayable.IPlayable)Activator.CreateInstance(tournamentPlayers[lbIABlack.SelectedIndex].GetType());
            UI.Title = "TEST RUNNING";
            Tuple<int, int> move;
            bool blackTurn = true;
            int percent = 0;
            for (int t =0; t<100; t++)
            {
                // generate a random board position
                int[,] testBoard = getRandomBoard();
                
                move = Referee.GetNextMove(testBoard, 5, blackTurn);
                if (move.Item1 != -1)
                {
                    testBlackPlayer.PlayMove(move.Item1, move.Item2, blackTurn);
                    //verify
                    Referee.PlayMove(move.Item1, move.Item2, blackTurn);
                    int[,] board1 = testBlackPlayer.GetBoard();
                    int[,] board2 = Referee.GetBoard();
                    if (boardCompare(board1, board2))
                        percent++;
                    UI.Title += ".";
                }
                else
                {
                    percent++;
                    UI.Title += "X";
                }
                System.Threading.Thread.Sleep(30);
            }

            UI.Title = $"TEST COMPLETE ({percent}%)";
        }

        /// <summary>
        /// Returns a random board position for testing purpose
        /// </summary>
        /// <returns></returns>
        private int[,] getRandomBoard()
        {
            Random rnd = new Random();
            int[,] board = new int[8, 8];
            for (int i=0; i<8;i++)
            {
                for (int j=0;j<8;j++)
                {
                    int v = rnd.Next(-5, 6);
                    if (v == 0)
                        board[i, j] = -1;
                    else if (v < 0)
                        board[i, j] = 0;
                    else board[i, j] = 1;
                }
            }
            return board;
        }

        private void gameSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            delay = (int)gameSpeed.Value;
        }

        private void MenuItem_About(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(@"OTHELLO IA PLAYER TOURNAMENT APP (HE-ARC 2021)

The IA must implement the IPlayable interface and be in the executable directory
1. Find IA will load all OthelloIA.dll assemblies
2. Select two IA (one for each player) from the populated list
3. Start game 

a slider is available at the bottom to slow down(100-5000ms) the game if required");
        }
    }
}
