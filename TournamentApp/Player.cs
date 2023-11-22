using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.Timers;
using System.Windows.Threading;
using System.Runtime.Serialization;
using static Othello.Pawn;

namespace Othello
{
    /// <summary>
    /// Classe représentant un joueur
    /// </summary>
    [Serializable()]
    public class Player : ISerializable
    {
        string name;
        Pawn.Colors color;
        TimeSpan maxTime = new TimeSpan(0, 05, 00);
        public TimeSpan timeLeft;
        private bool timeMoving = false;

        /// <summary>
        /// Constructeur avec paramètres
        /// </summary>
        /// <param name="n"></param>
        /// <param name="c"></param>
        public Player(string n, Colors c)
        {
            timeLeft = maxTime;
            name = n;
            color = c;
            
        }

        /// <summary>
        /// Méthode qui marque le joueur comme joueur courant, donc celui à qui c'est le tour.
        /// </summary>
        public void start()
        {
            timeMoving = true;
        }

        /// <summary>
        /// Méthode qui marque le joueur comme étant plus le joueur courrant, ce n'est plus son tour.
        /// </summary>
        public void stop()
        {
            timeMoving = false;
        }

        /// <summary>
        /// Méthode qui incrémente l'horloge du joueur.
        /// </summary>
        public void tick()
        {
            if (timeMoving)
                timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));
        }

        public int SecondsLeft => (int) timeLeft.Seconds;
        public int TotalSecondsLeft => (int)timeLeft.TotalSeconds;
        public int MinutesLeft => (int)timeLeft.Minutes;

        //Serialization
        /// <summary>
        /// Méthode permettant la sérialisation, vient de l'interface ISerializable
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name",name);
            info.AddValue("Color", color);
            info.AddValue("TimeLeft", timeLeft);
            info.AddValue("timeMoving", timeMoving);
        }
        /// <summary>
        /// Constructeur de sérialisation
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public Player(SerializationInfo info, StreamingContext ctxt)
        {
            name = (string)info.GetValue("Name", typeof(string));
            color = (Colors)info.GetValue("Color", typeof(Colors));
            timeLeft = (TimeSpan)info.GetValue("TimeLeft", typeof(TimeSpan));
            timeMoving = (bool)info.GetValue("timeMoving", typeof(bool));
        }
        

        public void reset() => timeLeft = maxTime;
    }
}
