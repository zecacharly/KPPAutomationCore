using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KPPAutomationCore {


    public static class AcessManagement {

        public enum Acesslevel { Admin, User, NotSet, Man }


        public delegate void AcesslevelChanged(Acesslevel NewLevel);
        public static event AcesslevelChanged OnAcesslevelChanged;




        private static Acesslevel _acessLevel = Acesslevel.NotSet;

        public static Acesslevel AcessLevel {
            get { return _acessLevel; }
            set {
                if (_acessLevel != value) {
                    _acessLevel = value;
                    if (OnAcesslevelChanged != null) {
                        OnAcesslevelChanged(value);
                    }
                }
            }
        }
    }
}
