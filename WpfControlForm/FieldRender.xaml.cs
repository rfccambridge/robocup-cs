using RFC.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace RFC.WpfControlForm
{
    /// <summary>
    /// Interaction logic for FieldRender.xaml
    /// </summary>
    public partial class FieldRender : Window, INotifyPropertyChanged
    {
        #region Data Binding
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private Team _team;
        public Team Team {
            get { return _team; }
            set
            {
                var last = _team;
                _team = value;
                if (value != last)
                {
                    OnPropertyChanged(nameof(Team));
                    OnPropertyChanged(nameof(TeamName));
                    OnPropertyChanged(nameof(TeamColor));
                }
            }
        }
        public string TeamName => Team.ToString();
        public Color TeamColor => Team == Team.Blue ? Colors.Blue : Colors.Yellow;


        private string _refBoxCmd;
        public string RefBoxCmd {
            get { return _refBoxCmd; }
            set
            {
                var last = _refBoxCmd;
                _refBoxCmd = value;
                if (value != last)
                    OnPropertyChanged(nameof(RefBoxCmd));
            }
        }


        private string _playType;
        public string PlayType
        {
            get { return _playType; }
            set
            {
                var last = _playType;
                _playType = value;
                if (value != last)
                    OnPropertyChanged(nameof(PlayType));
            }
        }


        private string _playName;
        public string PlayName
        {
            get { return _playName; }
            set
            {
                var last = _playName;
                _playName = value;
                if (value != last)
                    OnPropertyChanged(nameof(PlayName));
            }
        }
        #endregion

        public FieldRender()
        {
            DataContext = this;
            InitializeComponent();
        }
    }
}
