using RFC.Core;
using RFC.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
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

namespace RFC.WpfControlForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            Teams = Enum.GetValues(typeof(Team))
                .Cast<Team>()
                .Select(e => new KeyValuePair<Team, string>(e, e.ToString()))
                .ToList();

            Connections = SerialPort.GetPortNames().ToList();
            SelectedConnection = "Simulator";
            Connections.Insert(0, SelectedConnection);

            DataContext = this;

            SelectedTeam = Team.Yellow;
        }

        public Team SelectedTeam { get; set; }
        public List<KeyValuePair<Team, string>> Teams { get; }

        public string SelectedConnection { get; set; }
        public List<string> Connections { get; }

        public bool Flipped { get; set; }

        public int GoalieId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _running;
        public bool Running {
            get { return _running; }
            set {
                _running = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Running)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Stopped)));
            }
        }
        public bool Stopped => !Running;
    }
}
