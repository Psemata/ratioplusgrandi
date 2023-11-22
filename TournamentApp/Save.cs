using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Othello
{
    /// <summary>
    /// Classe regroupant les différentes éléments à sauvegarder.
    /// </summary>
    [Serializable()]
    public class Save : ISerializable
    {
        public LogicBoard Board;
        public Player CurrentPlayer;
        public Player Player2;
        public int turn;

        /// <summary>
        /// Constructeur avec paramètres
        /// </summary>
        /// <param name="b"></param>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <param name="t"></param>
        public Save(LogicBoard b, Player current, Player other, int t)
        {
            Board = b;
            CurrentPlayer = current;
            Player2 = other;
            turn = t;
        }

        /// <summary>
        /// Méthode permettant la serialisation. Provient de ISerializale
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Board", Board);
            info.AddValue("CurrentPlayer", CurrentPlayer);
            info.AddValue("Player2", Player2);
            info.AddValue("Turn", turn);
        }
        /// <summary>
        /// Constructeur de sérialisation.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public Save(SerializationInfo info, StreamingContext ctxt)
        {
            Board = (LogicBoard)info.GetValue("Board", typeof(LogicBoard));
            CurrentPlayer = (Player)info.GetValue("CurrentPlayer", typeof(Player));
            Player2 = (Player)info.GetValue("Player2", typeof(Player));
            turn = (int)info.GetValue("Turn", typeof(int));
        }
    }
}
