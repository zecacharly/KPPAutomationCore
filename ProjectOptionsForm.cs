using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VisionModule;
using KPP.Core.Debug;
using BrightIdeasSoftware;
using System.Threading;
using System.Globalization;

namespace KPPAutomationCore {

    public partial class ProjectOptionsForm : Form {

        private ModuleProjects _Projsconf;

        public ModuleProjects Projsconf {
            get { return _Projsconf; }
            set { 
                _Projsconf = value; 
            }
        }

        //public ModuleProjects Projsconf = null;

        public String _projsfile = "";
        private static KPPLogger log = new KPPLogger(typeof(ProjectOptionsForm));
        Type ModuleType;
        public ProjectOptionsForm(Type moduleType) {
            ModuleType = moduleType;
            switch (LanguageSettings.Language) {
                case LanguageName.Unk:
                    break;
                case LanguageName.PT:
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt-PT");

                    break;
                case LanguageName.EN:
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("de");

                    break;
                default:
                    break;
            }
            InitializeComponent();
           
        }

        private void ProjectOptionsForm_FormClosing(object sender, FormClosingEventArgs e) {
            this.Hide();
            e.Cancel=true;
        }


        private void _btNewProj_Click(object sender, EventArgs e) {
            

        }

        private void __btsave_Click(object sender, EventArgs e) {
            Projsconf.WriteConfigurationFile();
        }

        private void __listprojects_CellEditFinishing(object sender, BrightIdeasSoftware.CellEditEventArgs e) {
            try {
                if (e.Cancel) {
                    return;
                }
                
                if (e.Column == olvLoadOnStart && e.RowObject != null) {

                    Boolean cellvalue = (Boolean)e.NewValue;
                    String projname = ((ModuleProject)e.RowObject).Name;

                    foreach (ModuleProject item in Projsconf.Projects) {
                        if (cellvalue == true) {
                            if (item.Name != projname) {
                                item.Loadonstart = false;
                            }
                        }
                    }

                    //e.Value
                }
                else if (e.Column == olvProjName) {
                    ((ModuleProject)e.RowObject).Name = (String)e.NewValue;                
                    
                } 
                __listprojects.RefreshObjects(Projsconf.Projects);
                List<ModuleProject> projects = __listprojects.Objects.Cast<ModuleProject>().ToList();
                Projsconf.Projects = projects;
                Projsconf.WriteConfigurationFile(_projsfile);
            }
            catch (Exception exp) {

                log.Error(exp);
            }
        }

        private void __btduplicate_Click(object sender, EventArgs e) {
            ModuleProject source = __listprojects.SelectedObject as ModuleProject;

            if (source!=null) {
                ModuleProject newproj = source.Clone() as ModuleProject;
                if (newproj!=null) {
                    newproj.Name = newproj.Name + "_copy";
                    Projsconf.Projects.Add(newproj);
                    __listprojects.Objects = Projsconf.Projects;
                }
            }
        }

        private void __listprojects_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode== Keys.Enter) {
                if (__listprojects.SelectedIndex>0) {
                    __btLoadProj.PerformClick();
                }
            }
        }

       

        private void ProjectOptionsForm_Load(object sender, EventArgs e) {
            AcessManagement.OnAcesslevelChanged += new AcessManagement.AcesslevelChanged(StaticObjects_OnAcesslevelChanged);
            
        }

        void StaticObjects_OnAcesslevelChanged(Acesslevel NewLevel) {
            Boolean state = NewLevel ==Acesslevel.Admin;

            olvLoadOnStart.IsVisible = state;
            olvProjID.IsVisible = state;
         
            __btduplicate.Visible = state;
            __btNewProj.Visible = state;

            __listprojects.RebuildColumns();

        }

       

        private void button1_Click(object sender, EventArgs e) {
            if (__listprojects.SelectedIndex==0) {
                __listprojects.SelectedIndex = __listprojects.Items.Count - 1;
            } else {
                __listprojects.SelectedIndex--;
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            if (__listprojects.SelectedIndex == __listprojects.Items.Count - 1) {
                __listprojects.SelectedIndex = 0;
            } else {
                __listprojects.SelectedIndex++;
            }
        }

        private void __btNewProj_Click(object sender, EventArgs e) {
            try {
                // TODO CHECK ModuleProject Type instanciation
                var newproject = Activator.CreateInstance(ModuleType);
                Projsconf.Projects.Add((ModuleProject)newproject);
                __listprojects.Objects=Projsconf.Projects;
                Projsconf.WriteConfigurationFile(_projsfile);

            } catch (Exception exp) {

                log.Error(exp);
            }
        }

      
    }
}
