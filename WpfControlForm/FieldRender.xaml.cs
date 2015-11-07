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
        protected virtual void OnPropertyChanged(String prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

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
        public String TeamName => Team.ToString();
        public Color TeamColor => Team == Team.Blue ? Colors.Blue : Colors.Yellow;


        private String _refBoxCmd;
        public String RefBoxCmd {
            get { return _refBoxCmd; }
            set
            {
                var last = _refBoxCmd;
                _refBoxCmd = value;
                if (value != last)
                    OnPropertyChanged(nameof(RefBoxCmd));
            }
        }


        private String _playType;
        public String PlayType
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


        private String _playName;
        public String PlayName
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
