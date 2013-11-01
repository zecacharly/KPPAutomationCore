using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KPPAutomationCore {



    public class UserDef {

        private Acesslevel m_Level = Acesslevel.NotSet;

        public Acesslevel Level {
            get { return m_Level; }
            set { m_Level = value; }
        }

        private String m_Pass = "";

        public String Pass {
            get { return m_Pass; }
            set { m_Pass = value; }
        }


        public UserDef(String pass, Acesslevel level) {
            Pass = pass;
            Level = level;
        }

        public UserDef() {

        }
    }


    public enum Acesslevel { Admin, User, NotSet, Man }

    public static class AcessManagement {

       


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
